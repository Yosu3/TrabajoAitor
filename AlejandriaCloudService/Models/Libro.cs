using System;
using System.Collections.Generic;
using System.Text;

namespace AlejandriaCloudService.Models
{
    public class Libro
    {
        public int idLibro { get; set; }
        public string titulo { get; set; } = string.Empty;
        public string autor { get; set; } = string.Empty;
        public string isbn { get; set; } = string.Empty;
        public string categoria { get; set; } = string.Empty;
        public string ubicacion { get; set; } = string.Empty;
        public string estado { get; set; } = "Disponible";
        public DateTime fechaPublicacion { get; set; } = DateTime.Now;
        public string imagenPortada { get; set; } = string.Empty;
    }
}