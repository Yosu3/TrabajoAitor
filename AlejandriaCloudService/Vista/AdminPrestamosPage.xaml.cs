using AlejandriaCloudService.Funcionalidades;
using AlejandriaCloudService.Models;

namespace AlejandriaCloudService.Vista;

public partial class AdminPrestamosPage : ContentPage
{
    public AdminPrestamosPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await UsuarioService.CargarDatosDesdeJsonAsync();
            ActualizarListas();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "Aceptar");
        }
    }

    private void ActualizarListas()
    {
        ListaSolicitudes.ItemsSource = null;
        ListaEntregados.ItemsSource = null;
        ListaSolicitudes.ItemsSource = UsuarioService.ObtenerSolicitudesPendientes();
        ListaEntregados.ItemsSource = UsuarioService.ObtenerPrestamosEntregados();
    }

    private async void OnEntregadoClicked(object? sender, EventArgs e)
    {
        if (sender is not Button boton || boton.CommandParameter is not Prestamo solicitud)
            return;

        boton.IsEnabled = false;
        try
        {
            bool entregado = await UsuarioService.EntregarLibroAsync(solicitud);
            await DisplayAlertAsync(entregado ? "Entregado" : "No permitido",
                entregado ? "El estado del libro es ahora Prestado." : "No se pudo entregar el libro.",
                "Aceptar");
            ActualizarListas();
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

    private async void OnDevolverClicked(object? sender, EventArgs e)
    {
        if (sender is not Button boton || boton.CommandParameter is not Prestamo prestamo)
            return;

        bool confirmar = await DisplayAlertAsync("Registrar devolución",
            $"¿Ha devuelto {prestamo.Username} el libro '{prestamo.Libro?.titulo}'?",
            "Devolver libro", "Cancelar");
        if (!confirmar)
            return;

        boton.IsEnabled = false;
        try
        {
            bool devuelto = await UsuarioService.RegistrarDevolucionAsync(prestamo);
            await DisplayAlertAsync(devuelto ? "Devuelto" : "No permitido",
                devuelto ? "El libro vuelve a estar disponible y deja de estar asignado al usuario." :
                "No se pudo registrar la devolución.", "Aceptar");
            ActualizarListas();
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

    private async void OnVolverClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}