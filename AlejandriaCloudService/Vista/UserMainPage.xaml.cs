using AlejandriaCloudService.Funcionalidades;
using AlejandriaCloudService.Models;
using System.Linq;

namespace AlejandriaCloudService.Vista;

public partial class UserMainPage : ContentPage
{
    private bool isPasswordVisible = false;
    private Entry PasswordEntry;
    private Button TogglePasswordButton;
    public UserMainPage(string username)
	{
		InitializeComponent();
        TxtBienvenida.Text = $"Hola, {username} 👋";
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await CatalogoService.CargarLibrosAsync();
            LibrosRecomendados.ItemsSource = CatalogoService.ObtenerLibros().Take(3).ToList();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error de carga", $"No se pudo leer el catálogo: {ex.Message}", "OK");
        }
    }

    private async void OnCatalogoTapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new CatalogoPage());
    }

    private async void OnPrestamosTapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new prestamosPage());
    }

    private async void OnVerMisLibrosClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new prestamosPage());
    }

    private async void OnDetalleLibroTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is Libro libro)
        {
            await Navigation.PushAsync(new DetalleLibroPage(libro));
        }
    }

    private void OnCambiarAreaTapped(object? sender, TappedEventArgs e)
    {
        Application.Current!.MainPage = new NavigationPage(new LoginPage());
    }

	


}