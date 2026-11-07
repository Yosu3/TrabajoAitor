using System.Text.RegularExpressions;
using AlejandriaCloudService.Funcionalidades;
using AlejandriaCloudService.Models;
using Microsoft.Maui.Storage;

namespace AlejandriaCloudService.Vista;

public partial class AnadirLibroPage : ContentPage
{
    private FileResult? archivoPortada;

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

    private void OnPortadaCheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (SeleccionarPortadaButton == null || NombrePortadaLabel == null || VistaPreviaPortada == null)
            return;

        SeleccionarPortadaButton.IsVisible = e.Value;
        if (!e.Value)
        {
            archivoPortada = null;
            NombrePortadaLabel.Text = "Se utilizará la portada predeterminada";
            VistaPreviaPortada.Source = "libro_imagen.png";
        }
        else
        {
            NombrePortadaLabel.Text = "Selecciona una imagen";
        }
    }

    private async void OnSeleccionarPortadaClicked(object? sender, EventArgs e)
    {
        try
        {
            FileResult? seleccionado = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Elige la portada del libro",
                FileTypes = FilePickerFileType.Images
            });

            if (seleccionado == null)
                return;

            archivoPortada = seleccionado;
            NombrePortadaLabel.Text = seleccionado.FileName;
            using Stream stream = await seleccionado.OpenReadAsync();
            using MemoryStream memoria = new();
            await stream.CopyToAsync(memoria);
            byte[] datos = memoria.ToArray();
            VistaPreviaPortada.Source = ImageSource.FromStream(() => new MemoryStream(datos));
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Imagen", ex.Message, "Aceptar");
        }
    }

    private async void GuardarClicked(object? sender, EventArgs e)
    {
        string titulo = TituloEntry.Text?.Trim() ?? "";
        string autor = AutorEntry.Text?.Trim() ?? "";
        string ubicacion = UbicacionEntry.Text?.Trim() ?? "";
        string isbn = IsbnEntry.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(autor) ||
            string.IsNullOrWhiteSpace(ubicacion))
        {
            await DisplayAlertAsync("Datos incompletos", "El título, el autor y la ubicación son obligatorios.", "Aceptar");
            return;
        }

        if (titulo.Length > 150 || autor.Length > 100 || ubicacion.Length > 100)
        {
            await DisplayAlertAsync("Datos no válidos", "Has superado la longitud máxima de un campo.", "Aceptar");
            return;
        }

        if (isbn.Length > 0 && !Regex.IsMatch(isbn, @"^\d{10}(\d{3})?$"))
        {
            await DisplayAlertAsync("ISBN no válido", "El ISBN debe tener 10 o 13 números.", "Aceptar");
            return;
        }

        if (CategoriaPicker.SelectedItem is not string categoria)
        {
            await DisplayAlertAsync("Falta categoría", "Elige una categoría.", "Aceptar");
            return;
        }

        if (PortadaCheckBox.IsChecked && archivoPortada == null)
        {
            await DisplayAlertAsync("Falta portada", "Selecciona una imagen o desmarca la casilla.", "Aceptar");
            return;
        }

        GuardarButton.IsEnabled = false;

        try
        {
            await CatalogoService.CargarLibrosAsync();

            if (AdminLibrosService.ExisteTitulo(titulo))
            {
                await DisplayAlertAsync("Título repetido", "Ya existe un libro con ese título.", "Aceptar");
                return;
            }

            string portada = "libro_imagen.png";
            if (PortadaCheckBox.IsChecked && archivoPortada != null)
                portada = await LibroPortadaService.GuardarPortadaAsync(archivoPortada);

            Libro nuevo = new()
            {
                titulo = titulo,
                autor = autor,
                categoria = categoria,
                ubicacion = ubicacion,
                isbn = isbn,
                fechaPublicacion = FechaPicker.Date is DateTime fecha ? fecha : DateTime.Today,
                imagenPortada = portada
            };

            await AdminLibrosService.AnadirLibroAsync(nuevo);
            await DisplayAlertAsync("Libro añadido", $"'{titulo}' se ha añadido al catálogo.", "Aceptar");
            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "Aceptar");
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
        PortadaCheckBox.IsChecked = false;
        archivoPortada = null;
        VistaPreviaPortada.Source = "libro_imagen.png";
        NombrePortadaLabel.Text = "Se utilizará la portada predeterminada";
    }

    private async void OnVolverInicioClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}