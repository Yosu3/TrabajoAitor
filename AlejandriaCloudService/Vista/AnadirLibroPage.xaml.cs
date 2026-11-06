using AlejandriaCloudService.Funcionalidades;
using AlejandriaCloudService.Models;

namespace AlejandriaCloudService.Vista;

public partial class AnadirLibroPage : ContentPage
{
    public AnadirLibroPage()
    {
        InitializeComponent();

        CategoriaPicker.ItemsSource = new List<string>
        {
            "Clásicos", "Novela", "Ciencia ficción", "Fantasía",
            "Historia", "Infantil", "Distopía", "Realismo Mágico"
        };
        FechaPicker.MaximumDate = DateTime.Today;
        FechaPicker.Date = DateTime.Today;
    }

    private async void GuardarClicked(object? sender, EventArgs e)
    {
        string titulo = TituloEntry.Text?.Trim() ?? "";
        string autor = AutorEntry.Text?.Trim() ?? "";
        string ubicacion = UbicacionEntry.Text?.Trim() ?? "";
        string isbn = IsbnEntry.Text?.Trim() ?? "";

        // Validaciones
        if (titulo == "")
        {
            await DisplayAlertAsync("Error", "El título no puede estar vacío.", "OK");
            return;
        }
        if (autor == "")
        {
            await DisplayAlertAsync("Error", "El autor no puede estar vacío.", "OK");
            return;
        }
        if (CategoriaPicker.SelectedItem is not string categoria)
        {
            await DisplayAlertAsync("Error", "Selecciona una categoría.", "OK");
            return;
        }
        if (ubicacion == "")
        {
            await DisplayAlertAsync("Error", "La ubicación no puede estar vacía.", "OK");
            return;
        }

        // Bloqueamos el botón para evitar que se pulse dos veces 
        GuardarButton.IsEnabled = false;

        try
        {
            await CatalogoService.CargarLibrosAsync();

            if (isbn != "" && AdminLibrosService.ExisteIsbn(isbn))
            {
                await DisplayAlertAsync("Error", "Ya existe un libro con ese ISBN.", "OK");
                return;
            }

            Libro nuevo = new Libro
            {
                titulo = titulo,
                autor = autor,
                categoria = categoria,
                ubicacion = ubicacion,
                isbn = isbn,
                estado = "Disponible",
                fechaPublicacion = FechaPicker.Date ?? DateTime.Today
            };

            await AdminLibrosService.AnadirLibroAsync(nuevo);

            await DisplayAlertAsync("Libro añadido", $"\"{titulo}\" se ha añadido con el id #{nuevo.idLibro}.", "OK");
            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo guardar el libro: {ex.Message}", "OK");
        }
        finally
        {
            GuardarButton.IsEnabled = true;
        }
    }

    private void LimpiarFormulario()
    {
        TituloEntry.Text = "";
        AutorEntry.Text = "";
        UbicacionEntry.Text = "";
        IsbnEntry.Text = "";
        CategoriaPicker.SelectedIndex = -1;
        FechaPicker.Date = DateTime.Today;
    }

    private async void OnVolverInicioClicked(object? sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }
}
