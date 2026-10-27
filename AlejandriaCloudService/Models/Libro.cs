using System;
using System.Collections.Generic;
using System.Text;

namespace BibliotecaAlejandria.Models
{
    public class Libro
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Autor { get; set; } = string.Empty;
        public string Isbn { get; set; } = string.Empty;
        public bool IsDisponible { get; set; } = true;
    }
}
