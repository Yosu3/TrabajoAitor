using BibliotecaAlejandria.Models;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace AlejandriaCloudService.Vista;

public partial class NewPage1 : ContentPage
{
    // Campo de clase para almacenar la lista de usuarios recuperada del JSON
    private List<Usuario> _usuariosDePrueba = new List<Usuario>();

    public NewPage1()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarUsuariosDesdeJsonAsync();
    }

    private async Task CargarUsuariosDesdeJsonAsync()
    {
        try
        {
            // Abre y lee correctamente el archivo desde Resources/Raw/Ficheros/Usuarios.json
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("Ficheros/Usuarios.json");
            using StreamReader reader = new StreamReader(stream);
            string jsonContenido = await reader.ReadToEndAsync();

            var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _usuariosDePrueba = JsonSerializer.Deserialize<List<Usuario>>(jsonContenido, opciones) ?? new List<Usuario>();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error de datos", $"No se pudo cargar la base de usuarios: {ex.Message}", "OK");
        }
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        string username = txtUsername.Text?.Trim() ?? string.Empty;
        string password = txtPassword.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlertAsync("Campos Incompletos", "Por favor, introduce tu usuario y contraseña.", "Entendido");
            return;
        }

        if (username.Length < 4)
        {
            await DisplayAlertAsync("Usuario no válido", "El nombre de usuario debe contener al menos 4 caracteres.", "OK");
            return;
        }

        var usuarioEncontrado = _usuariosDePrueba.FirstOrDefault(u =>
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (usuarioEncontrado == null || usuarioEncontrado.Password != password)
        {
            await DisplayAlertAsync("Error de Acceso", "Usuario o contraseña incorrectos.", "Reintentar");
            return;
        }

        if (usuarioEncontrado.IsAdmin)
        {
            await DisplayAlertAsync("¡Bienvenido Panel de Control!", $"Administrador: {usuarioEncontrado.Username}", "Ingresar");
        }
        else
        {
            await DisplayAlertAsync("¡Bienvenido de vuelta!", $"Hola {usuarioEncontrado.Username}, que pases una excelente lectura.", "Entrar");
        }
    }

    private void OnTogglePasswordClicked(object? sender, EventArgs e)
    {
        txtPassword.IsPassword = !txtPassword.IsPassword;
        btnTogglePassword.Text = txtPassword.IsPassword ? "👁️" : "🙈";
    }

    private async void OnRegisterTapped(object? sender, TappedEventArgs e)
    {
        await DisplayAlertAsync("Registro", "Redirigiendo a la pantalla de creación de cuenta...", "OK");
    }
}
