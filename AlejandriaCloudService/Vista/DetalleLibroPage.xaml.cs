using AlejandriaCloudService.Funcionalidades;
using AlejandriaCloudService.Models;

namespace AlejandriaCloudService.Vista;

public partial class DetalleLibroPage : ContentPage
{
    private readonly Libro _libro;

    public DetalleLibroPage(Libro libro)
    {
        InitializeComponent();
        _libro = libro;

        TituloLabel.Text = libro.titulo;
        AutorLabel.Text = $"Autor: {libro.autor}";
        CategoriaLabel.Text = $"Categoría: {libro.categoria}";
        IsbnLabel.Text = $"ISBN: {libro.isbn}";
        UbicacionLabel.Text = $"Ubicación: {libro.ubicacion}";
        PublicacionLabel.Text = $"Publicado en: {libro.fechaPublicacion.Year}";
        EstadoLabel.Text = $"Estado: {libro.estado}";

        if (libro.estado == "Disponible")
        {
            EstadoLabel.TextColor = Colors.Green;
            DisponibilidadLabel.Text = "Este libro está disponible para pedirlo prestado.";
        }
        else if (libro.estado == "Prestado")
        {
            EstadoLabel.TextColor = Colors.Red;
            DisponibilidadLabel.Text = "Este libro está prestado ahora mismo.";
        }
        else
        {
            EstadoLabel.TextColor = Colors.Orange;
            DisponibilidadLabel.Text = "Este libro está reservado.";
        }
    }

    private async void EliminarClicked(object? sender, EventArgs e)
    {
        bool confirmar = await DisplayAlertAsync(
            "Eliminar libro",
            $"¿Seguro que deseas eliminar el libro \"{_libro.titulo}\"?",
            "Eliminar", "Cancelar");

        if (!confirmar)
        {
            return;
        }

        bool eliminado = await AdminLibrosService.EliminarLibroAsync(_libro);
        if (eliminado)
        {
            await DisplayAlertAsync("Eliminado", $"\"{_libro.titulo}\" ha sido eliminado del catálogo.", "OK");
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlertAsync("Error", "No se pudo eliminar el libro.", "OK");
        }
    }

    private async void VolverClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
