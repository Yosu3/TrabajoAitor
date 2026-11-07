using AlejandriaCloudService.Funcionalidades;
using AlejandriaCloudService.Models;

namespace AlejandriaCloudService.Vista;

public partial class CatalogoPage : ContentPage
{
    public CatalogoPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await UsuarioService.CargarDatosDesdeJsonAsync();
            ActualizarLista();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "Aceptar");
        }
    }

    private void ActualizarLista()
    {
        bool esAdmin = UsuarioService.UsuarioActual?.IsAdmin == true;
        ListaAdmin.IsVisible = esAdmin;
        ListaUsuarios.IsVisible = !esAdmin;
        List<Libro> libros = CatalogoService.BuscarLibros(BuscadorLibros.Text ?? "", esAdmin);
        ListaUsuarios.ItemsSource = null;
        ListaAdmin.ItemsSource = null;
        if (esAdmin)
            ListaAdmin.ItemsSource = libros;
        else
            ListaUsuarios.ItemsSource = libros;
    }

    private void OnBuscarTextChanged(object? sender, TextChangedEventArgs e)
    {
        ActualizarLista();
    }

    private async void OnVerDetalleTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is Libro libro)
            await Navigation.PushAsync(new DetalleLibroPage(libro));
    }

    private async void OnCambiarDisponibilidadClicked(object? sender, EventArgs e)
    {
        if (sender is not Button boton || boton.CommandParameter is not Libro libro ||
            UsuarioService.UsuarioActual?.IsAdmin != true)
            return;

        bool reactivar = libro.estado == "No disponible";
        string accion = reactivar ? "reactivar" : "retirar";
        bool confirmar = await DisplayAlertAsync("Confirmar cambio",
            $"¿Quieres {accion} el libro '{libro.titulo}'?",
            "Confirmar", "Cancelar");

        if (!confirmar)
            return;

        boton.IsEnabled = false;
        try
        {
            bool realizado = reactivar ?
                await AdminLibrosService.ReactivarLibroAsync(libro) :
                await AdminLibrosService.EliminarLibroAsync(libro);

            await DisplayAlertAsync(realizado ? "Estado actualizado" : "No permitido",
                realizado ? $"El libro está ahora {(reactivar ? "Disponible" : "No disponible")}." :
                "No es posible retirar un libro con una solicitud o préstamo activo.",
                "Aceptar");
            ActualizarLista();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "Aceptar");
        }
        finally
        {
            boton.IsEnabled = true;
        }
    }

    private async void OnVolverInicioClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}