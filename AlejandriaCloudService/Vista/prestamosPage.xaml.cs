using AlejandriaCloudService.Funcionalidades;
using AlejandriaCloudService.Models;

namespace AlejandriaCloudService.Vista;

public partial class prestamosPage : ContentPage
{
    public prestamosPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await UsuarioService.CargarDatosDesdeJsonAsync();

            if (UsuarioService.UsuarioActual?.IsAdmin != false)
            {
                await DisplayAlertAsync("Acceso denegado", "Solo los lectores pueden solicitar libros.", "Aceptar");
                await Navigation.PopAsync();
                return;
            }

            ActualizarLista();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "Aceptar");
        }
    }

    private void ActualizarLista()
    {
        List<Prestamo> misPrestamos = UsuarioService.ObtenerMisSolicitudesYPrestamos();

        int cantidadLibros = UsuarioService.ObtenerCantidadLibrosActivos();

        bool limiteAlcanzado = cantidadLibros >= UsuarioService.MaximoLibrosPorUsuario;

        TxtBienvenida.Text = $"Hola, {UsuarioService.UsuarioActual?.Username} | Libros: {cantidadLibros}/3";

        PkrLibrosDisponibles.ItemsSource = null;
        ListadoPrestamos.ItemsSource = null;

        PkrLibrosDisponibles.ItemsSource = UsuarioService.ObtenerLibrosDisponibles();
        PkrLibrosDisponibles.SelectedIndex = -1;

        ListadoPrestamos.ItemsSource = misPrestamos;

        PkrLibrosDisponibles.IsEnabled = !limiteAlcanzado;
        SolicitarButton.IsEnabled = !limiteAlcanzado;

        SolicitarButton.Text = limiteAlcanzado
            ? "Límite de 3 libros alcanzado"
            : "Enviar solicitud";
    }

    private async void OnSolicitarPrestamoClicked(object? sender, EventArgs e)
    {
        int cantidadLibros = UsuarioService.ObtenerCantidadLibrosActivos();

        if (cantidadLibros >= UsuarioService.MaximoLibrosPorUsuario)
        {
            await DisplayAlertAsync(
                "Límite alcanzado",
                "Solo puedes tener un máximo de 3 libros entre solicitudes y préstamos. Debes devolver un libro antes de solicitar otro.",
                "Aceptar");
            return;
        }

        if (PkrLibrosDisponibles.SelectedItem is not Libro libro)
        {
            await DisplayAlertAsync("Falta información", "Selecciona un libro.", "Aceptar");
            return;
        }

        SolicitarButton.IsEnabled = false;

        try
        {
            bool solicitado = await UsuarioService.SolicitarPrestamoAsync(libro.idLibro);

            if (solicitado)
            {
                await DisplayAlertAsync(
                    "Solicitud enviada",
                    "El libro queda En solicitud hasta que el administrador lo entregue.",
                    "Aceptar");
            }
            else if (UsuarioService.ObtenerCantidadLibrosActivos() >= UsuarioService.MaximoLibrosPorUsuario)
            {
                await DisplayAlertAsync(
                    "Límite alcanzado",
                    "Ya tienes 3 libros entre solicitudes y préstamos. No puedes solicitar más.",
                    "Aceptar");
            }
            else
            {
                await DisplayAlertAsync(
                    "No disponible",
                    "Este libro ya no se puede solicitar.",
                    "Aceptar");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "Aceptar");
        }
        finally
        {
            ActualizarLista();
        }
    }

    private async void OnVolverInicioClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
