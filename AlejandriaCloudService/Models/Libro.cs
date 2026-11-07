using System.Text.Json.Serialization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace AlejandriaCloudService.Models;

public class Libro
{
    public int idLibro { get; set; }
    public string titulo { get; set; } = "";
    public string autor { get; set; } = "";
    public string isbn { get; set; } = "";
    public string categoria { get; set; } = "";
    public string ubicacion { get; set; } = "";
    public string estado { get; set; } = "Disponible";
    public DateTime fechaPublicacion { get; set; } = DateTime.Today;
    public string imagenPortada { get; set; } = "libro_imagen.png";

    [JsonIgnore]
    public string AccionCatalogo => estado == "No disponible" ? "Reactivar" : "Retirar";

    [JsonIgnore]
    public ImageSource PortadaSource
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(imagenPortada))
            {
                string ruta = imagenPortada.Replace('/', Path.DirectorySeparatorChar);

                if (Path.IsPathRooted(ruta) && File.Exists(ruta))
                    return ImageSource.FromFile(ruta);

                if (imagenPortada.StartsWith("portadas/", StringComparison.OrdinalIgnoreCase))
                {
                    string archivo = Path.Combine(FileSystem.Current.AppDataDirectory, ruta);
                    if (File.Exists(archivo))
                        return ImageSource.FromFile(archivo);
                }
            }

            return ImageSource.FromFile("libro_imagen.png");
        }
    }
}