using System.Text.Json;
using AlejandriaCloudService.Models;
using Microsoft.Maui.Storage;

namespace AlejandriaCloudService.Funcionalidades
{
    public static class CatalogoService
    {
        public static List<Libro> Libros { get; private set; } = new List<Libro>();

        private static readonly string ruta =
            Path.Combine(FileSystem.AppDataDirectory, "Libros.json");

        private static bool cargado = false;

        public static async Task CargarLibrosAsync()
        {
            if (cargado)
            {
                return; 
            }

            if (!File.Exists(ruta))
            {
                try
                {
                    using Stream stream = await FileSystem.OpenAppPackageFileAsync("Ficheros/Libros.json");
                    using StreamReader reader = new StreamReader(stream);
                    string inicial = await reader.ReadToEndAsync();
                    await File.WriteAllTextAsync(ruta, inicial);
                }
                catch
                {
                    Libros = new List<Libro>();
                    cargado = true;
                    return;
                }
            }

            string json = await File.ReadAllTextAsync(ruta);
            JsonSerializerOptions opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            Libros = JsonSerializer.Deserialize<List<Libro>>(json, opciones) ?? new List<Libro>();
            cargado = true;
        }

        public static async Task GuardarLibrosAsync()
        {
            await CargarLibrosAsync(); 

            JsonSerializerOptions opciones = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(Libros, opciones);
            await File.WriteAllTextAsync(ruta, json);
        }

        public static List<Libro> ObtenerLibros()
        {
            return new List<Libro>(Libros);
        }

        public static List<Libro> BuscarLibros(string texto)
        {
            texto = (texto ?? "").Trim();
            List<Libro> resultado = new List<Libro>();

            foreach (Libro libro in Libros)
            {
                bool coincide = texto == ""
                    || libro.titulo.Contains(texto, StringComparison.OrdinalIgnoreCase)
                    || libro.autor.Contains(texto, StringComparison.OrdinalIgnoreCase)
                    || libro.categoria.Contains(texto, StringComparison.OrdinalIgnoreCase);

                if (coincide)
                {
                    resultado.Add(libro);
                }
            }
            return resultado;
        }
    }
}
