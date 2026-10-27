using AlejandriaCloudService.Models;
using System.Text.Json;

namespace AlejandriaCloudService.Vista;

public partial class CatalogoPage : ContentPage
{
    public static List<Libro> LibrosBase { get; set; } = new List<Libro>();
    public CatalogoPage()
	{
		InitializeComponent();
        CargarLibrosDesdeJson();
    }

    private async void CargarLibrosDesdeJson()
    {
        try
        {
            // Nota la L mayúscula corregida en la ruta del paquete de la app
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("Ficheros/Libros.json");
            using StreamReader reader = new StreamReader(stream);

            string jsonTexto = await reader.ReadToEndAsync();

            var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            LibrosBase = JsonSerializer.Deserialize<List<Libro>>(jsonTexto, opciones) ?? new List<Libro>();

            ListaLibros.ItemsSource = LibrosBase;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error de carga", $"No se pudo leer el archivo: {ex.Message}", "OK");
        }
    }
    private void OnBuscarTextChanged(object sender, TextChangedEventArgs e)
    {
        string textoBusqueda = e.NewTextValue;

        if (string.IsNullOrWhiteSpace(textoBusqueda))
        {
            ListaLibros.ItemsSource = LibrosBase;
            return;
        }

        string filtro = textoBusqueda.ToLower().Trim();

        var librosFiltrados = LibrosBase.Where(l =>
            (l.titulo != null && l.titulo.ToLower().Contains(filtro)) ||
            (l.autor != null && l.autor.ToLower().Contains(filtro)) ||
            (l.categoria != null && l.categoria.ToLower().Contains(filtro))
        ).ToList();

        ListaLibros.ItemsSource = librosFiltrados;
    }
}
