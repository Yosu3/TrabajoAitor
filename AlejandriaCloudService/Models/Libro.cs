namespace BibliotecaAlejandria.Models
{
    public class Prestamo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string LibroId { get; set; } = string.Empty;  
        public string Username { get; set; } = string.Empty;  
        public DateTime FechaPrestamo { get; set; } = DateTime.Now;
    }
}
