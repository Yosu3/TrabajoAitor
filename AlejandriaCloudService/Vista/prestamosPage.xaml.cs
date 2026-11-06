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
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error de carga", $"No se pudo leer Libros.json: {ex.Message}", "OK");
            return;
        }

        if (UsuarioService.UsuarioActual != null)
            TxtBienvenida.Text = $"¡Hola, {UsuarioService.UsuarioActual.Username}!";

        RefrescarControlesPantalla();
    }

    private void RefrescarControlesPantalla()
    {
        PkrLibrosDisponibles.ItemsSource = null;
        ListadoPrestamos.ItemsSource = null;

        PkrLibrosDisponibles.ItemsSource = UsuarioService.ObtenerLibrosDisponibles();
        PkrLibrosDisponibles.SelectedIndex = -1;

        if (UsuarioService.UsuarioActual != null)
        {
            ListadoPrestamos.ItemsSource = UsuarioService.UsuarioActual.Prestamos;
        }
    }

    private async void OnSolicitarPrestamoClicked(object sender, EventArgs e)
    {
        var libroSeleccionado = PkrLibrosDisponibles.SelectedItem as Libro;

        if (libroSeleccionado == null)
        {
            // RESTAURADO: Usando tu método obligatorio del proyecto
            await DisplayAlertAsync("Atención", "Por favor, selecciona un libro de la lista desplegable.", "OK");
            return;
        }

         bool exito = await UsuarioService.SolicitarPrestamoAsync(libroSeleccionado.idLibro);

        if (exito)
        {
            await DisplayAlertAsync("Éxito", $"Has tomado en préstamo: '{libroSeleccionado.titulo}'.", "Entendido");
            RefrescarControlesPantalla();   // antes: RefrescarControlesPantacle()
        }
        else
        {
            await DisplayAlertAsync("Error", "No se pudo procesar el préstamo.", "Cerrar");
        }
    }

    private async void OnDevolverLibroClicked(object sender, EventArgs e)
    {
        var boton = sender as Button;

        if (boton?.CommandParameter is int idLibro)
        {
            
            bool confirmar = await DisplayAlertAsync("Devolución", "¿Deseas devolver este ejemplar a la colección?", "Sí, Confirmar", "Cancelar");

            if (!confirmar) return;

             bool exito = await UsuarioService.DevolverLibroAsync(idLibro);

            if (exito)
            {
                await DisplayAlertAsync("Completado", "El libro ha sido entregado correctamente.", "OK");
                RefrescarControlesPantalla();
            }
            else
            {
                await DisplayAlertAsync("Error", "No se pudo procesar la entrega.", "Cerrar");
            }
        }
    }

    private async void OnVolverInicioClicked(object? sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }
    }
