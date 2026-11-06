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
            await CatalogoService.CargarLibrosAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error de carga", $"No se pudo leer el archivo: {ex.Message}", "OK");
            return;
        }

        ActualizarLista();
    }

    private void ActualizarLista()
    {
        ListaLibros.ItemsSource = CatalogoService.BuscarLibros(BuscadorLibros.Text ?? "");
    }

    private void OnBuscarTextChanged(object? sender, TextChangedEventArgs e)
    {
        ActualizarLista();
    }

    private async void OnVolverInicioClicked(object? sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }

    private async void OnVerDetalleTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is Libro libro)
        {
            await Navigation.PushAsync(new DetalleLibroPage(libro));
        }
    }

    private async void OnEliminarClicked(object? sender, EventArgs e)
    {
        if (sender is not Button boton || boton.CommandParameter is not Libro libro)
        {
            return;
        }

        bool confirmar = await DisplayAlertAsync(
            "Eliminar libro",
            $"¿Seguro que deseas eliminar el libro \"{libro.titulo}\"?",
            "Eliminar", "Cancelar");

        if (!confirmar)
        {
            return;
        }

        bool eliminado = await AdminLibrosService.EliminarLibroAsync(libro);
        if (eliminado)
        {
            await DisplayAlertAsync("Eliminado", $"\"{libro.titulo}\" ha sido eliminado del catálogo.", "OK");
            ActualizarLista();
        }
        else
        {
            await DisplayAlertAsync("Error", "No se encontró el libro para eliminar.", "OK");
        }
    }
}
