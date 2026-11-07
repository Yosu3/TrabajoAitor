using AlejandriaCloudService.Funcionalidades;
using AlejandriaCloudService.Models;

namespace AlejandriaCloudService.Vista;

public partial class UserMainPage : ContentPage
{
    public UserMainPage(string username)
    {
        InitializeComponent();
        TxtBienvenida.Text = $"Hola, {username}";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await UsuarioService.CargarDatosDesdeJsonAsync();
            LibrosRecomendados.ItemsSource = CatalogoService.ObtenerLibros();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "Aceptar");
        }
    }

    private async void OnCatalogoClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new CatalogoPage());
    }

    private async void OnPrestamosClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new prestamosPage());
    }

    private async void OnDetalleLibroTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is Libro libro)
            await Navigation.PushAsync(new DetalleLibroPage(libro));
    }

    private void OnSalirClicked(object? sender, EventArgs e)
    {
        UsuarioService.UsuarioActual = null;
        Application.Current!.MainPage = new NavigationPage(new LoginPage());
    }
}