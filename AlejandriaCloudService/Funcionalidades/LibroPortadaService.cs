using Microsoft.Maui.Storage;

namespace AlejandriaCloudService.Funcionalidades;

public static class LibroPortadaService
{
    public static async Task<string> GuardarPortadaAsync(FileResult archivo)
    {
        string extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        string[] extensiones = { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp", ".heic" };

        if (!extensiones.Contains(extension))
            throw new InvalidOperationException("El archivo seleccionado no es una imagen admitida.");

        string carpeta = Path.Combine(FileSystem.Current.AppDataDirectory, "portadas");
        Directory.CreateDirectory(carpeta);
        string nombre = $"{Guid.NewGuid():N}{extension}";
        string destino = Path.Combine(carpeta, nombre);

        using Stream origen = await archivo.OpenReadAsync();
        using FileStream salida = File.Create(destino);
        await origen.CopyToAsync(salida);

        return $"portadas/{nombre}";
    }
}