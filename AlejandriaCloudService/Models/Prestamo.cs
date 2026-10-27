using System;
using System.Collections.Generic;
using System.Text;

namespace AlejandriaCloudService.Models
{
    public class Prestamo
    {
        public int Id { get; set; }
        public int LibroId { get; set; }
        public Libro Libro { get; set; } = null!;
        public string Username { get; set; } = string.Empty;

        public DateTime FechaSalida { get; set; } = DateTime.Now;
        public DateTime FechaLimite { get; set; } = DateTime.Now.AddDays(14);
        public bool EstabaDevuelto { get; set; } = false;
    }
}
