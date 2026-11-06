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

        private static readonly string RutaUsuarios =
            Path.Combine(FileSystem.AppDataDirectory, "Usuarios.json");

        public static async Task CargarDatosDesdeJsonAsync()
        {
            await CatalogoService.CargarLibrosAsync();

            if (!File.Exists(RutaUsuarios))
            {
                using Stream stream = await FileSystem.OpenAppPackageFileAsync("Ficheros/Usuarios.json");
                using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                await File.WriteAllTextAsync(RutaUsuarios, await reader.ReadToEndAsync());
            }

            string usuariosJson = await File.ReadAllTextAsync(RutaUsuarios);
            var usuarios = JsonSerializer.Deserialize<List<Usuario>>(usuariosJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Usuario>();

            if (UsuarioActual != null)
            {
                Usuario? usuarioPersistido = usuarios.FirstOrDefault(u =>
                    u.Username.Equals(UsuarioActual.Username, StringComparison.OrdinalIgnoreCase));

                if (usuarioPersistido != null)
                {
                    UsuarioActual = usuarioPersistido;
                }
            }

            CatalogoGlobal = CatalogoService.ObtenerLibros();

            foreach (Prestamo prestamo in UsuarioActual?.Prestamos ?? new List<Prestamo>())
            {
                prestamo.Libro = CatalogoGlobal.FirstOrDefault(l => l.idLibro == prestamo.LibroId)
                    ?? prestamo.Libro;
            }

            if (UsuarioActual != null)
            {
                UsuarioActual.LibrosAsignados = UsuarioActual.Prestamos
                    .Where(p => p.Libro != null)
                    .Select(p => p.Libro)
                    .ToList();
            }
        }

        public static List<Libro> ObtenerLibrosDisponibles()
        {
            return CatalogoGlobal.Where(l => l.estado.Equals("Disponible", StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public static async Task<bool> SolicitarPrestamoAsync(int idLibro)
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

            await GuardarUsuariosAsync();
            await CatalogoService.GuardarLibrosAsync();
            return true;
        }

        public static async Task<bool> DevolverLibroAsync(int idLibro)
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

            await GuardarUsuariosAsync();
            await CatalogoService.GuardarLibrosAsync();
            return true;
        }

        private static async Task GuardarUsuariosAsync()
        {
            if (!File.Exists(RutaUsuarios)) return;

            string json = await File.ReadAllTextAsync(RutaUsuarios);
            var usuarios = JsonSerializer.Deserialize<List<Usuario>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Usuario>();

            Usuario? usuarioPersistido = usuarios.FirstOrDefault(u =>
                u.Username.Equals(UsuarioActual.Username, StringComparison.OrdinalIgnoreCase));

            if (usuarioPersistido == null) return;

            usuarioPersistido.Prestamos = UsuarioActual.Prestamos;
            usuarioPersistido.LibrosAsignados = UsuarioActual.LibrosAsignados;

            await File.WriteAllTextAsync(RutaUsuarios, JsonSerializer.Serialize(usuarios,
                new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}