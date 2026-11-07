using AlejandriaCloudService.Funcionalidades;
using AlejandriaCloudService.Models;

namespace AlejandriaCloudService.Vista;

public partial class UserMainPage : ContentPage
{
    private List<Libro> _librosVisibles = new();

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
            _librosVisibles = CatalogoService.ObtenerLibros();

            List<string> categorias = _librosVisibles
                .Select(libro => libro.categoria.Trim())
                .Where(categoria => !string.IsNullOrWhiteSpace(categoria))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(categoria => categoria)
                .ToList()!;

            categorias.Insert(0, "Todas");
            CategoriaPicker.ItemsSource = categorias;
            CategoriaPicker.SelectedIndex = 0;
            AplicarFiltroCategoria();
        }

        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "Aceptar");
        }
    }

    private void OnCategoriaChanged(object? sender, EventArgs e)
    {
        AplicarFiltroCategoria();
    }

    private void AplicarFiltroCategoria()
    {
        string? categoriaSeleccionada = CategoriaPicker.SelectedItem as string;

        LibrosRecomendados.ItemsSource = string.IsNullOrWhiteSpace(categoriaSeleccionada) ||
            categoriaSeleccionada.Equals("Todas", StringComparison.OrdinalIgnoreCase)
            ? _librosVisibles
            : _librosVisibles.Where(libro => libro.categoria.Equals(
                categoriaSeleccionada, StringComparison.OrdinalIgnoreCase)).ToList();
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