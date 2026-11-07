using AlejandriaCloudService.Funcionalidades;
using AlejandriaCloudService.Models;

namespace AlejandriaCloudService.Vista;

public partial class DetalleLibroPage : ContentPage
{
    private readonly Libro libro;

    public DetalleLibroPage(Libro libroSeleccionado)
    {
        InitializeComponent();
        libro = libroSeleccionado;
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

        GestionarLibroButton.IsVisible = UsuarioService.UsuarioActual?.IsAdmin == true;
        GestionarLibroButton.Text = libro.estado == "No disponible" ? "Reactivar libro" : "Retirar libro";
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

    private async void VolverClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}