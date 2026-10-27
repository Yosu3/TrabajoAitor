using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Generic;

namespace BibliotecaAlejandria.Models
{
    public class Usuario
    {
        public bool IsAdmin { get; set; } = false;
        public string Username { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;

        public List<Libro> LibrosAsignados { get; set; } = new List<Libro>();
        public List<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
        public List<Devolucion> Devoluciones { get; set; } = new List<Devolucion>();
    }
}
