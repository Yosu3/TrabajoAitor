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

        public static void AnadirLibro(Libro libro)
        {
            libro.idLibro = SiguienteId();
            CatalogoService.Libros.Add(libro);
        }
    }
}
