using AlejandriaCloudService.Models;

namespace AlejandriaCloudService.Funcionalidades;

public static class AdminLibrosService
{
    public static int SiguienteId()
    {
        return CatalogoService.Libros.Select(l => l.idLibro).DefaultIfEmpty(0).Max() + 1;
    }

    public static bool ExisteTitulo(string titulo)
    {
        return CatalogoService.Libros.Any(l =>
            string.Equals(l.titulo.Trim(), titulo.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public static async Task AnadirLibroAsync(Libro libro)
    {
        await CatalogoService.CargarLibrosAsync();

        if (UsuarioService.UsuarioActual?.IsAdmin != true)
            throw new InvalidOperationException("Solo un administrador puede añadir libros.");

        if (string.IsNullOrWhiteSpace(libro.titulo) || ExisteTitulo(libro.titulo))
            throw new InvalidOperationException("Ya existe un libro con ese título o el título está vacío.");

        libro.idLibro = SiguienteId();
        libro.estado = "Disponible";
        CatalogoService.Libros.Add(libro);
        await CatalogoService.GuardarLibrosAsync();
    }

    public static async Task<bool> EliminarLibroAsync(Libro libro)
    {
        await UsuarioService.CargarDatosDesdeJsonAsync();

        if (UsuarioService.UsuarioActual?.IsAdmin != true)
            return false;

        Libro? existente = CatalogoService.Libros.FirstOrDefault(l => l.idLibro == libro.idLibro);
        if (existente == null || existente.estado == "No disponible" ||
            UsuarioService.TieneRelacionActiva(existente.idLibro))
            return false;

        existente.estado = "No disponible";
        await CatalogoService.GuardarLibrosAsync();
        return true;
    }

    public static async Task<bool> ReactivarLibroAsync(Libro libro)
    {
        await UsuarioService.CargarDatosDesdeJsonAsync();

        if (UsuarioService.UsuarioActual?.IsAdmin != true)
            return false;

        Libro? existente = CatalogoService.Libros.FirstOrDefault(l => l.idLibro == libro.idLibro);
        if (existente?.estado != "No disponible" ||
            UsuarioService.TieneRelacionActiva(existente.idLibro))
            return false;

        existente.estado = "Disponible";
        await CatalogoService.GuardarLibrosAsync();
        return true;
    }
}