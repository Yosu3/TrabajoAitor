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

    private async void OnLibroSeleccionado(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count == 0)
        {
            return;
        }

        Libro? libro = e.CurrentSelection[0] as Libro;
        if (libro is null)
        {
            return;
        }

        ListaLibros.SelectedItem = null;   
        await Navigation.PushAsync(new DetalleLibroPage(libro));
    }
}
