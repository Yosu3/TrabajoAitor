using System.Text.Json;
using AlejandriaCloudService.Models;
using Microsoft.Maui.Storage;

namespace AlejandriaCloudService.Funcionalidades;

internal static class UsuarioService
{
    public const int MaximoLibrosPorUsuario = 3;
    public static Usuario? UsuarioActual { get; set; }
    public static List<Usuario> Usuarios { get; private set; } = new();

    private static readonly string ruta =
        Path.Combine(FileSystem.Current.AppDataDirectory, "Usuarios.json");

    public static async Task CargarDatosDesdeJsonAsync()
    {
        await CatalogoService.CargarLibrosAsync();

        if (!File.Exists(ruta))
        {
            using Stream stream = await FileSystem.Current.OpenAppPackageFileAsync("Ficheros/Usuarios.json");
            using StreamReader reader = new(stream);
            await File.WriteAllTextAsync(ruta, await reader.ReadToEndAsync());
        }

        string json = await File.ReadAllTextAsync(ruta);
        Usuarios = JsonSerializer.Deserialize<List<Usuario>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

        foreach (Usuario usuario in Usuarios)
        {
            usuario.Prestamos ??= new();
            usuario.LibrosAsignados ??= new();
            usuario.Devoluciones ??= new();

            if (usuario.IsAdmin)
            {
                usuario.Prestamos.Clear();
                usuario.LibrosAsignados.Clear();
                continue;
            }

            foreach (Prestamo prestamo in usuario.Prestamos)
            {
                prestamo.Libro = CatalogoService.Libros.FirstOrDefault(l => l.idLibro == prestamo.LibroId);
                prestamo.Username = usuario.Username;
            }

            usuario.LibrosAsignados = usuario.Prestamos
                .Where(p => p.Libro != null && p.Libro.estado == "Prestado")
                .Select(p => p.Libro!)
                .ToList();
        }

        if (UsuarioActual != null)
            UsuarioActual = Usuarios.FirstOrDefault(u =>
                u.Username.Equals(UsuarioActual.Username, StringComparison.OrdinalIgnoreCase));
    }

    public static List<Libro> ObtenerLibrosDisponibles()
    {
        return CatalogoService.Libros
            .Where(l => l.estado.Equals("Disponible", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public static List<Prestamo> ObtenerMisSolicitudesYPrestamos()
    {
        if (UsuarioActual == null || UsuarioActual.IsAdmin)
            return new();

        return UsuarioActual.Prestamos
            .Where(p => p.Libro != null && (p.Libro.estado == "En solicitud" || p.Libro.estado == "Prestado"))
            .OrderByDescending(p => p.FechaSolicitud)
            .ToList();
    }

    public static int ObtenerCantidadLibrosActivos()
    {
        return ObtenerMisSolicitudesYPrestamos()
            .Select(p => p.LibroId)
            .Distinct()
            .Count();
    }

    public static List<Prestamo> ObtenerSolicitudesPendientes()
    {
        if (UsuarioActual?.IsAdmin != true)
            return new();

        return Usuarios.Where(u => !u.IsAdmin)
            .SelectMany(u => u.Prestamos)
            .Where(p => p.Libro?.estado == "En solicitud")
            .OrderBy(p => p.FechaSolicitud)
            .ToList();
    }

    public static List<Prestamo> ObtenerPrestamosEntregados()
    {
        if (UsuarioActual?.IsAdmin != true)
            return new();

        return Usuarios.Where(u => !u.IsAdmin)
            .SelectMany(u => u.Prestamos)
            .Where(p => p.Libro?.estado == "Prestado")
            .OrderBy(p => p.FechaLimite)
            .ToList();
    }

    public static bool TieneRelacionActiva(int libroId)
    {
        return Usuarios.Any(u => u.Prestamos.Any(p =>
            p.LibroId == libroId && !p.EstabaDevuelto));
    }

    public static async Task<bool> SolicitarPrestamoAsync(int libroId)
    {
        await CargarDatosDesdeJsonAsync();

        if (UsuarioActual == null || UsuarioActual.IsAdmin)
            return false;

        if (ObtenerCantidadLibrosActivos() >= MaximoLibrosPorUsuario)
            return false;

        Libro? libro = CatalogoService.Libros.FirstOrDefault(l => l.idLibro == libroId);

        if (libro == null || libro.estado != "Disponible" || TieneRelacionActiva(libroId))
            return false;

        int siguienteId = Usuarios
            .SelectMany(u => u.Prestamos)
            .Select(p => p.Id)
            .DefaultIfEmpty(0)
            .Max() + 1;

        UsuarioActual.Prestamos.Add(new Prestamo
        {
            Id = siguienteId,
            LibroId = libroId,
            Libro = libro,
            Username = UsuarioActual.Username,
            FechaSolicitud = DateTime.Now
        });

        libro.estado = "En solicitud";

        await GuardarDatosAsync();

        return true;
    }

    public static async Task<bool> EntregarLibroAsync(Prestamo solicitud)
    {
        await CargarDatosDesdeJsonAsync();

        if (UsuarioActual?.IsAdmin != true)
            return false;

        Usuario? usuario = Usuarios.FirstOrDefault(u => !u.IsAdmin &&
            u.Username.Equals(solicitud.Username, StringComparison.OrdinalIgnoreCase));
        Prestamo? prestamo = usuario?.Prestamos.FirstOrDefault(p => p.Id == solicitud.Id &&
            p.LibroId == solicitud.LibroId);
        Libro? libro = CatalogoService.Libros.FirstOrDefault(l => l.idLibro == solicitud.LibroId);

        if (usuario == null || prestamo == null || libro?.estado != "En solicitud")
            return false;

        prestamo.FechaSalida = DateTime.Now;
        prestamo.FechaLimite = DateTime.Now.AddDays(14);
        libro.estado = "Prestado";

        if (!usuario.LibrosAsignados.Any(l => l.idLibro == libro.idLibro))
            usuario.LibrosAsignados.Add(libro);

        await GuardarDatosAsync();
        return true;
    }

    public static async Task<bool> RegistrarDevolucionAsync(Prestamo prestamoEntregado)
    {
        await CargarDatosDesdeJsonAsync();

        if (UsuarioActual?.IsAdmin != true)
            return false;

        Usuario? usuario = Usuarios.FirstOrDefault(u => !u.IsAdmin &&
            u.Username.Equals(prestamoEntregado.Username, StringComparison.OrdinalIgnoreCase));
        Prestamo? prestamo = usuario?.Prestamos.FirstOrDefault(p => p.Id == prestamoEntregado.Id &&
            p.LibroId == prestamoEntregado.LibroId);
        Libro? libro = CatalogoService.Libros.FirstOrDefault(l => l.idLibro == prestamoEntregado.LibroId);

        if (usuario == null || prestamo == null || libro?.estado != "Prestado")
            return false;

        usuario.Prestamos.Remove(prestamo);
        usuario.LibrosAsignados.RemoveAll(l => l.idLibro == libro.idLibro);
        libro.estado = "Disponible";

        await GuardarDatosAsync();
        return true;
    }

    private static async Task GuardarDatosAsync()
    {
        string json = JsonSerializer.Serialize(Usuarios,
            new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(ruta, json);
        await CatalogoService.GuardarLibrosAsync();
    }
}