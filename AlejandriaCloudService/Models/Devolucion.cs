using System;
using System.Collections.Generic;
using System.Text;

namespace AlejandriaCloudService.Models
{
    public class Devolucion
    {
        public int Id { get; set; }

        public int PrestamoId { get; set; }
        public Prestamo Prestamo { get; set; } = null!;

        public DateTime FechaEntrega { get; set; } = DateTime.Now;
        public string Observaciones { get; set; } = string.Empty; // Por si el libro viene dañado
    }
}

