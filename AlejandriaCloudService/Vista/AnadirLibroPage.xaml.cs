using AlejandriaCloudService.Funcionalidades;
using AlejandriaCloudService.Models;
using System.Text.RegularExpressions;

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
        if (string.IsNullOrWhiteSpace(titulo))
        {
            await DisplayAlertAsync("Error", "El título no puede estar vacío.", "OK");
            return;
        }

        if (titulo.Length > 150 || autor.Length > 100 || ubicacion.Length > 100)
        {
            await DisplayAlertAsync("Error", "El título, autor o ubicación supera la longitud permitida.", "OK");
            return;
        }

        if (isbn != "" && !Regex.IsMatch(isbn, @"^\d{10}(\d{3})?$"))
        {
            await DisplayAlertAsync("ISBN no válido", "El ISBN debe contener 10 o 13 dígitos, sin espacios ni guiones.", "OK");
            return;
        }
        if (string.IsNullOrWhiteSpace(autor))
        {
            await DisplayAlertAsync("Error", "El autor no puede estar vacío.", "OK");
            return;
        }
        if (CategoriaPicker.SelectedItem is not string categoria)
        {
            await DisplayAlertAsync("Error", "Selecciona una categoría.", "OK");
            return;
        }
        if (string.IsNullOrWhiteSpace(ubicacion))
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
