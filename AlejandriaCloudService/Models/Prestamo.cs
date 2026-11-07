using System.Text.Json.Serialization;

namespace AlejandriaCloudService.Models;

public class Prestamo
{
    public int Id { get; set; }
    public int LibroId { get; set; }
    public string Username { get; set; } = "";
    public DateTime FechaSolicitud { get; set; } = DateTime.Now;
    public DateTime FechaSalida { get; set; }
    public DateTime FechaLimite { get; set; }
    public bool EstabaDevuelto { get; set; }

    [JsonIgnore]
    public Libro? Libro { get; set; }
}