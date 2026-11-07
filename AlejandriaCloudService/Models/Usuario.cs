namespace AlejandriaCloudService.Models;

public class Usuario
{
    public bool IsAdmin { get; set; }
    public string Username { get; set; } = "";
    public string Apellido { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Dni { get; set; } = "";
    public List<Libro> LibrosAsignados { get; set; } = new();
    public List<Prestamo> Prestamos { get; set; } = new();
    public List<Devolucion> Devoluciones { get; set; } = new();
}