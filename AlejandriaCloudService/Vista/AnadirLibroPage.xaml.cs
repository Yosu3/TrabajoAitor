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

        try
        {
            await CatalogoService.CargarLibrosAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error de carga", $"No se pudo leer el archivo: {ex.Message}", "OK");
            return;
        }

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
        AdminLibrosService.AnadirLibro(nuevo);

        await DisplayAlertAsync("Libro añadido", $"\"{titulo}\" se ha añadido con el id {nuevo.idLibro}.", "OK");
        LimpiarFormulario();
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
}
