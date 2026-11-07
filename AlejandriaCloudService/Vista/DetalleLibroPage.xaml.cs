using AlejandriaCloudService.Funcionalidades;
using AlejandriaCloudService.Models;
using Microsoft.Maui.Storage;

namespace AlejandriaCloudService.Vista;

public partial class DetalleLibroPage : ContentPage
{
    private readonly Libro libro;
    private FileResult? nuevaPortada;
    private bool usarPortadaPredeterminada;

    public DetalleLibroPage(Libro libroSeleccionado)
    {
        InitializeComponent();
        libro = libroSeleccionado;
        EditarFechaPicker.MaximumDate = DateTime.Today;
        MostrarLibro();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        MostrarLibro();
    }

    private void MostrarLibro()
    {
        PortadaImagen.Source = libro.PortadaSource;
        TituloLabel.Text = libro.titulo;
        AutorLabel.Text = $"Autor: {libro.autor}";
        CategoriaLabel.Text = $"Categoría: {libro.categoria}";
        IsbnLabel.Text = $"ISBN: {libro.isbn}";
        UbicacionLabel.Text = $"Ubicación: {libro.ubicacion}";
        PublicacionLabel.Text = $"Año: {libro.fechaPublicacion.Year}";
        EstadoLabel.Text = $"Estado: {libro.estado}";
        DisponibilidadLabel.Text = libro.estado switch
        {
            "Disponible" => "Se puede solicitar el libro.",
            "En solicitud" => "Esperando a que el administrador entregue el libro.",
            "Prestado" => "El libro está asignado a un lector.",
            "No disponible" => "Este libro está oculto para los lectores.",
            _ => "Consulta con el administrador."
        };

        bool esAdmin = UsuarioService.UsuarioActual?.IsAdmin == true;
        bool esLector = UsuarioService.UsuarioActual?.IsAdmin == false;
        bool disponible = libro.estado == "Disponible";
        bool limiteAlcanzado = UsuarioService.ObtenerCantidadLibrosActivos()>=UsuarioService.MaximoLibrosPorUsuario;
        SolicitarLibroButton.IsVisible = esLector && disponible;
        SolicitarLibroButton.IsEnabled = !limiteAlcanzado;
        SolicitarLibroButton.Text = limiteAlcanzado?"Límite de 3 libros alcanzado":"Solicitar libro";
        PanelEdicion.IsVisible = false;
        PanelDetalles.IsVisible = true;
        EditarLibroButton.IsVisible = esAdmin;
        GestionarLibroButton.IsVisible = esAdmin;
        GestionarLibroButton.Text = libro.estado == "No disponible" ? "Reactivar libro" : "Retirar libro";
    }

    private void OnEditarLibroClicked(object? sender, EventArgs e)
    {
        if (UsuarioService.UsuarioActual?.IsAdmin != true)
            return;

        EditarTituloEntry.Text = libro.titulo;
        EditarAutorEntry.Text = libro.autor;
        EditarIsbnEntry.Text = libro.isbn;
        EditarFechaPicker.Date = libro.fechaPublicacion.Date > DateTime.Today
            ? DateTime.Today
            : libro.fechaPublicacion;

        List<string> categorias = OpcionesBiblioteca.Categorias
            .Append(libro.categoria)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c)
            .ToList();
        EditarCategoriaPicker.ItemsSource = categorias;
        EditarCategoriaPicker.SelectedItem = categorias
            .FirstOrDefault(c => c.Equals(libro.categoria, StringComparison.OrdinalIgnoreCase));

        List<string> estanterias = OpcionesBiblioteca.Estanterias
            .Append(libro.ubicacion)
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        EditarUbicacionPicker.ItemsSource = estanterias;
        EditarUbicacionPicker.SelectedItem = estanterias
            .FirstOrDefault(u => u.Equals(libro.ubicacion, StringComparison.OrdinalIgnoreCase));

        nuevaPortada = null;
        usarPortadaPredeterminada = false;
        VistaPreviaPortada.Source = libro.PortadaSource;
        NombrePortadaLabel.Text = "Portada actual";

        PanelDetalles.IsVisible = false;
        PanelEdicion.IsVisible = true;
        EditarLibroButton.IsVisible = false;
        GestionarLibroButton.IsVisible = false;
    }

    private async void OnCambiarPortadaClicked(object? sender, EventArgs e)
    {
        if (UsuarioService.UsuarioActual?.IsAdmin != true)
            return;

        try
        {
            FileResult? seleccionada = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Selecciona una nueva portada",
                FileTypes = FilePickerFileType.Images
            });

            if (seleccionada == null)
                return;

            using Stream stream = await seleccionada.OpenReadAsync();
            using MemoryStream memoria = new();
            await stream.CopyToAsync(memoria);
            byte[] datos = memoria.ToArray();

            nuevaPortada = seleccionada;
            usarPortadaPredeterminada = false;
            VistaPreviaPortada.Source = ImageSource.FromStream(() => new MemoryStream(datos));
            NombrePortadaLabel.Text = seleccionada.FileName;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Portada", ex.Message, "Aceptar");
        }
    }

    private void OnPortadaPredeterminadaClicked(object? sender, EventArgs e)
    {
        if (UsuarioService.UsuarioActual?.IsAdmin != true)
            return;

        nuevaPortada = null;
        usarPortadaPredeterminada = true;
        VistaPreviaPortada.Source = "libro_imagen.png";
        NombrePortadaLabel.Text = "Se usará la portada predeterminada";
    }

    private async void OnGuardarEdicionClicked(object? sender, EventArgs e)
    {
        if (UsuarioService.UsuarioActual?.IsAdmin != true)
            return;

        string titulo = EditarTituloEntry.Text?.Trim() ?? "";
        string autor = EditarAutorEntry.Text?.Trim() ?? "";
        string isbn = EditarIsbnEntry.Text?.Trim() ?? "";

        if (EditarCategoriaPicker.SelectedItem is not string categoria ||
            EditarUbicacionPicker.SelectedItem is not string ubicacion)
        {
            await DisplayAlertAsync("Datos incompletos", "Selecciona la categoría y la estantería.", "Aceptar");
            return;
        }

        GuardarEdicionButton.IsEnabled = false;

        try
        {
            if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(autor))
            {
                await DisplayAlertAsync("Datos incompletos", "El título y el autor son obligatorios.", "Aceptar");
                return;
            }

            if (titulo.Length > 150 || autor.Length > 100 || ubicacion.Length > 100)
            {
                await DisplayAlertAsync("Datos no válidos", "Has superado la longitud máxima de un campo.", "Aceptar");
                return;
            }

            if (isbn.Length > 0 && !System.Text.RegularExpressions.Regex.IsMatch(isbn, @"^\d{10}(\d{3})?$"))
            {
                await DisplayAlertAsync("ISBN no válido", "El ISBN debe contener 10 o 13 dígitos.", "Aceptar");
                return;
            }

            if (AdminLibrosService.ExisteTitulo(titulo) &&
                !libro.titulo.Trim().Equals(titulo, StringComparison.OrdinalIgnoreCase))
            {
                await DisplayAlertAsync("Título repetido", "Ya existe otro libro con ese título.", "Aceptar");
                return;
            }

            string portada = usarPortadaPredeterminada ? "libro_imagen.png" : libro.imagenPortada;

            if (nuevaPortada != null)
                portada = await LibroPortadaService.GuardarPortadaAsync(nuevaPortada);

            Libro cambios = new()
            {
                idLibro = libro.idLibro,
                titulo = titulo,
                autor = autor,
                categoria = categoria,
                isbn = isbn,
                ubicacion = ubicacion,
                fechaPublicacion = EditarFechaPicker.Date is DateTime fecha ? fecha : DateTime.Today,
                imagenPortada = portada
            };

            await AdminLibrosService.EditarLibroAsync(cambios);
            nuevaPortada = null;
            usarPortadaPredeterminada = false;
            MostrarLibro();
            await DisplayAlertAsync("Libro actualizado", "Los cambios se han guardado correctamente.", "Aceptar");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "Aceptar");
        }
        finally
        {
            GuardarEdicionButton.IsEnabled = true;
        }
    }

    private void OnCancelarEdicionClicked(object? sender, EventArgs e)
    {
        nuevaPortada = null;
        usarPortadaPredeterminada = false;
        MostrarLibro();
    }

    private async void OnGestionarLibroClicked(object? sender, EventArgs e)
    {
        if (UsuarioService.UsuarioActual?.IsAdmin != true)
            return;

        bool reactivar = libro.estado == "No disponible";
        bool confirmar = await DisplayAlertAsync("Confirmar",
            $"¿Quieres {(reactivar ? "reactivar" : "retirar")} '{libro.titulo}'?",
            "Confirmar", "Cancelar");
        if (!confirmar)
            return;

        bool realizado = reactivar ?
            await AdminLibrosService.ReactivarLibroAsync(libro) :
            await AdminLibrosService.EliminarLibroAsync(libro);

        await DisplayAlertAsync(realizado ? "Actualizado" : "No permitido",
            realizado ? "La disponibilidad del libro se ha actualizado." :
            "No se puede retirar un libro con una solicitud o préstamo activo.", "Aceptar");
        MostrarLibro();
    }


    private async void OnSolicitarLibroClicked(object? sender, EventArgs e)
    {
        if (UsuarioService.UsuarioActual?.IsAdmin != false)
            return;

        if (libro.estado != "Disponible")
        {
            await DisplayAlertAsync(
                "Libro no disponible",
                "Este libro ya no se puede solicitar.",
                "Aceptar");

            MostrarLibro();
            return;
        }

        if (UsuarioService.ObtenerCantidadLibrosActivos() >=
            UsuarioService.MaximoLibrosPorUsuario)
        {
            await DisplayAlertAsync(
                "Límite alcanzado",
                "Ya tienes 3 libros entre solicitudes y préstamos. Debes devolver uno antes de solicitar otro.",
                "Aceptar");

            MostrarLibro();
            return;
        }

        bool confirmar = await DisplayAlertAsync(
            "Solicitar libro",
            $"¿Quieres solicitar el libro '{libro.titulo}'?",
            "Solicitar",
            "Cancelar");

        if (!confirmar)
            return;

        SolicitarLibroButton.IsEnabled = false;

        try
        {
            bool solicitado = await UsuarioService.SolicitarPrestamoAsync(libro.idLibro);

            if (solicitado)
            {
                await DisplayAlertAsync(
                    "Solicitud enviada",
                    $"Has solicitado '{libro.titulo}'. El libro queda En solicitud hasta que el administrador lo entregue.",
                    "Aceptar");
            }
            else if (UsuarioService.ObtenerCantidadLibrosActivos() >=
                     UsuarioService.MaximoLibrosPorUsuario)
            {
                await DisplayAlertAsync(
                    "Límite alcanzado",
                    "No puedes tener más de 3 libros entre solicitudes y préstamos.",
                    "Aceptar");
            }
            else
            {
                await DisplayAlertAsync(
                    "Solicitud no realizada",
                    "Este libro ya no está disponible para solicitar.",
                    "Aceptar");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                ex.Message,
                "Aceptar");
        }
        finally
        {
            MostrarLibro();
        }
    }


    private async void VolverClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}