using AlejandriaCloudService.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace AlejandriaCloudService.Funcionalidades
{
    internal class UsuarioService
    {
        public static List<Libro> CatalogoGlobal { get; set; } = new List<Libro>();
        public static Usuario UsuarioActual { get; set; }

        static UsuarioService()
        {
            UsuarioActual = new Usuario
            {
                Username = "Nai",
                Email = "nai@alejandria.com",
                IsAdmin = false
            };
        }

        public static async Task CargarDatosDesdeJsonAsync()
        {
            if (CatalogoGlobal.Count > 0) return;

            using Stream stream = await FileSystem.OpenAppPackageFileAsync("Ficheros/Libros.json");
            using StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8);    
            string jsonString = (await reader.ReadToEndAsync()).Trim('\uFEFF', '\u200B');

            var opciones = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };

            CatalogoGlobal = JsonSerializer.Deserialize<List<Libro>>(jsonString, opciones)
                             ?? new List<Libro>();

            foreach (var libro in CatalogoGlobal.Where(l =>
                         l.estado.Equals("Prestado", StringComparison.OrdinalIgnoreCase)))
            {
                if (UsuarioActual.LibrosAsignados.Any(l => l.idLibro == libro.idLibro)) continue;

                UsuarioActual.LibrosAsignados.Add(libro);
                UsuarioActual.Prestamos.Add(new Prestamo
                {
                    Id = libro.idLibro,
                    LibroId = libro.idLibro,
                    Libro = libro,
                    Username = UsuarioActual.Username,
                    FechaSalida = DateTime.Now,
                    EstabaDevuelto = false
                });
            }
        }

        public static List<Libro> ObtenerLibrosDisponibles()
        {
            return CatalogoGlobal.Where(l => l.estado.Equals("Disponible", StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public static bool SolicitarPrestamo(int idLibro)
        {
            if (UsuarioActual == null) return false;

            var libro = CatalogoGlobal.FirstOrDefault(l => l.idLibro == idLibro);
            if (libro == null || !libro.estado.Equals("Disponible", StringComparison.OrdinalIgnoreCase)) return false;

            libro.estado = "Prestado";
            UsuarioActual.LibrosAsignados.Add(libro);

            var transaccion = new Prestamo
            {
                Id = idLibro,
                LibroId = libro.idLibro,
                Libro = libro,
                Username = UsuarioActual.Username,
                FechaSalida = DateTime.Now,
                EstabaDevuelto = false
            };
            UsuarioActual.Prestamos.Add(transaccion);

            return true;
        }

        public static bool DevolverLibro(int idLibro)
        {
            if (UsuarioActual == null) return false;

            var libro = CatalogoGlobal.FirstOrDefault(l => l.idLibro == idLibro);
            if (libro != null)
            {
                libro.estado = "Disponible";
            }

            var libroAsignado = UsuarioActual.LibrosAsignados.FirstOrDefault(l => l.idLibro == idLibro);
            if (libroAsignado != null) UsuarioActual.LibrosAsignados.Remove(libroAsignado);

            var prestamoActivo = UsuarioActual.Prestamos.FirstOrDefault(p => p.LibroId == idLibro);
            if (prestamoActivo != null) UsuarioActual.Prestamos.Remove(prestamoActivo);

            return true;
        }
    }
}