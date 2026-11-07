using System.Text.Json;
using AlejandriaCloudService.Models;
using Microsoft.Maui.Storage;

namespace AlejandriaCloudService.Funcionalidades;

public static class CatalogoService
{
    public static List<Libro> Libros { get; private set; } = new();

    private static readonly string ruta =
        Path.Combine(FileSystem.Current.AppDataDirectory, "Libros.json");

    private static bool cargado;

    public static async Task CargarLibrosAsync()
    {
        if (cargado)
            return;

        if (!File.Exists(ruta))
        {
            using Stream stream = await FileSystem.Current.OpenAppPackageFileAsync("Ficheros/Libros.json");
            using StreamReader reader = new(stream);
            await File.WriteAllTextAsync(ruta, await reader.ReadToEndAsync());
        }

        string json = await File.ReadAllTextAsync(ruta);
        Libros = JsonSerializer.Deserialize<List<Libro>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        cargado = true;
    }

    public static async Task GuardarLibrosAsync()
    {
        await CargarLibrosAsync();
        string json = JsonSerializer.Serialize(Libros,
            new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(ruta, json);
    }

    public static List<Libro> ObtenerLibros(bool incluirNoDisponibles = false)
    {
        return Libros.Where(l => incluirNoDisponibles ||
            !l.estado.Equals("No disponible", StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public static List<Libro> BuscarLibros(string texto, bool incluirNoDisponibles = false)
    {
        texto = (texto ?? "").Trim();

        return ObtenerLibros(incluirNoDisponibles).Where(l =>
            l.titulo.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
            l.autor.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
            l.categoria.Contains(texto, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}