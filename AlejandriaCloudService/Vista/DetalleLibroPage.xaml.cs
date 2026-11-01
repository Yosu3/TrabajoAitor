using AlejandriaCloudService.Models;

namespace AlejandriaCloudService.Vista;

public partial class DetalleLibroPage : ContentPage
{
    public DetalleLibroPage(Libro libro)
    {
        InitializeComponent();

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

    private async void VolverClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
