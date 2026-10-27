namespace BibliotecaAlejandria.Models
{
    public class Libro
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Titulo { get; set; } = string.Empty;
        public string Autor { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Sinopsis { get; set; } = string.Empty;
        public bool IsDisponible { get; set; } = true;
    }
}
