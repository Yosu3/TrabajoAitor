using AlejandriaCloudService.Funcionalidades;

namespace AlejandriaCloudService.Vista;

public partial class AdminMainPage : ContentPage
{
    public AdminMainPage(string username)
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
            SolicitudesLabel.Text = $"Solicitudes pendientes: {UsuarioService.ObtenerSolicitudesPendientes().Count}";
            PrestadosLabel.Text = $"Libros prestados: {UsuarioService.ObtenerPrestamosEntregados().Count}";
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

    private async void OnAnadirLibroClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new AnadirLibroPage());
    }

    private async void OnPrestamosClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new AdminPrestamosPage());
    }

    private void OnSalirClicked(object? sender, EventArgs e)
    {
        UsuarioService.UsuarioActual = null;
        Application.Current!.MainPage = new NavigationPage(new LoginPage());
    }
}