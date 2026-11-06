using AlejandriaCloudService.Funcionalidades;
using AlejandriaCloudService.Models;
using System.Linq;

namespace AlejandriaCloudService.Vista;

public partial class AdminMainPage : ContentPage
{
	public AdminMainPage()
	{
		InitializeComponent();
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

	private async void OnAnadirLibroTapped(object? sender, TappedEventArgs e)
	{
		await Navigation.PushAsync(new AnadirLibroPage());
	}

	private async void OnPrestamosTapped(object? sender, TappedEventArgs e)
	{
		await Navigation.PushAsync(new prestamosPage());
	}

	private void OnCambiarAreaTapped(object? sender, TappedEventArgs e)
	{
		Application.Current!.MainPage = new NavigationPage(new LoginPage());
	}
	private async void OnDetalleLibroTapped(object? sender, TappedEventArgs e)
    {
		if (e.Parameter is Libro libro)
		{
			await Navigation.PushAsync(new DetalleLibroPage(libro));
		}
    }
}