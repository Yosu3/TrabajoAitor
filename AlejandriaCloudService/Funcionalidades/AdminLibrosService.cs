using AlejandriaCloudService.Models;

namespace AlejandriaCloudService.Funcionalidades
{
    public static class AdminLibrosService
    {
        // idLibro único: siempre el más alto + 1
        public static int SiguienteId()
        {
            int max = 0;
            foreach (Libro libro in CatalogoService.Libros)
            {
                if (libro.idLibro > max)
                {
                    max = libro.idLibro;
                }
            }
            return max + 1;
        }

        public static bool ExisteIsbn(string isbn)
        {
            foreach (Libro libro in CatalogoService.Libros)
            {
                if (libro.isbn == isbn)
                {
                    return true;
                }
            }
            return false;
        }

        public static async Task AnadirLibroAsync(Libro libro)
        {
            await CatalogoService.CargarLibrosAsync();
            libro.idLibro = SiguienteId();
            CatalogoService.Libros.Add(libro);
            await CatalogoService.GuardarLibrosAsync(); // Persistencia como los apuntes de Aitor con misioens
        }

        public static async Task<bool> EliminarLibroAsync(Libro libro)
        {
            await CatalogoService.CargarLibrosAsync();

            Libro? aBorrar = CatalogoService.Libros.FirstOrDefault(l => l.idLibro == libro.idLibro) ?? libro;
            bool eliminado = CatalogoService.Libros.Remove(aBorrar);

            if (eliminado)
            {
                await CatalogoService.GuardarLibrosAsync(); 
            }

            return eliminado;
        }
    }
}
