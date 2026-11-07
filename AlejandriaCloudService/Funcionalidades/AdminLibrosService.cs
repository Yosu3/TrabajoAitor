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

    public static async Task EditarLibroAsync(Libro cambios)
    {
        await CatalogoService.CargarLibrosAsync();

        if (UsuarioService.UsuarioActual?.IsAdmin != true)
            throw new InvalidOperationException("Solo un administrador puede editar libros.");

        Libro? existente = CatalogoService.Libros.FirstOrDefault(l => l.idLibro == cambios.idLibro);

        if (existente == null)
            throw new InvalidOperationException("El libro ya no existe en el catálogo.");

        string titulo = cambios.titulo.Trim();
        string autor = cambios.autor.Trim();
        string categoria = cambios.categoria.Trim();
        string ubicacion = cambios.ubicacion.Trim();
        string isbn = cambios.isbn.Trim();

        if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(autor) ||
            string.IsNullOrWhiteSpace(categoria) || string.IsNullOrWhiteSpace(ubicacion))
            throw new InvalidOperationException("Completa el título, autor, categoría y ubicación.");

        if (titulo.Length > 150 || autor.Length > 100 || ubicacion.Length > 100)
            throw new InvalidOperationException("Has superado la longitud máxima de un campo.");

        if (isbn.Length > 0 && !System.Text.RegularExpressions.Regex.IsMatch(isbn, @"^\d{10}(\d{3})?$"))
            throw new InvalidOperationException("El ISBN debe tener 10 o 13 dígitos.");

        if (cambios.fechaPublicacion.Date > DateTime.Today)
            throw new InvalidOperationException("La fecha de publicación no puede ser futura.");

        if (CatalogoService.Libros.Any(l => l.idLibro != existente.idLibro &&
            l.titulo.Trim().Equals(titulo, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Ya existe otro libro con ese título.");

        existente.titulo = titulo;
        existente.autor = autor;
        existente.categoria = categoria;
        existente.ubicacion = ubicacion;
        existente.isbn = isbn;
        existente.fechaPublicacion = cambios.fechaPublicacion;
        existente.imagenPortada = cambios.imagenPortada;

        await CatalogoService.GuardarLibrosAsync();
    }
}