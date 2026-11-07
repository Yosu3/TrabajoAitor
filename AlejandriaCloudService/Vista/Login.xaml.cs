using AlejandriaCloudService.Models;
using AlejandriaCloudService.Funcionalidades;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

namespace AlejandriaCloudService.Vista;

public partial class LoginPage : ContentPage
{
    private readonly string _filePath;

    public LoginPage()
    {
        InitializeComponent();
        _filePath = Path.Combine(FileSystem.AppDataDirectory, "Usuarios.json");
        InicializarArchivoJson();
    }

    private void InicializarArchivoJson()
    {
        if (!File.Exists(_filePath))
        {
            try
            {
                using Stream stream = FileSystem.OpenAppPackageFileAsync("Ficheros/Usuarios.json").Result;
                using StreamReader reader = new StreamReader(stream);
                string jsonContenidoOriginal = reader.ReadToEnd();

                var listaOriginal = JsonSerializer.Deserialize<List<Usuario>>(jsonContenidoOriginal) ?? new List<Usuario>();
                foreach (var usuario in listaOriginal)
                {
                    if (usuario.Password.Length < 30)
                    {
                        usuario.Password = EncriptarContrasena(usuario.Password);
                    }
                }

                string jsonStringProcesado = JsonSerializer.Serialize(listaOriginal, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, jsonStringProcesado);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string usernameInput = txtUsername.Text?.Trim();
        string passwordInput = txtPassword.Text;

        if (string.IsNullOrWhiteSpace(usernameInput) || string.IsNullOrWhiteSpace(passwordInput))
        {
            await DisplayAlert("Campos Vacíos", "Por favor, introduce tu usuario y contraseña.", "OK");
            return;
        }

        if (usernameInput.Length < 4)
        {
            await DisplayAlert("Usuario no válido", "El nombre de usuario debe contener al menos 4 caracteres.", "OK");
            return;
        }

        try
        {
            if (!File.Exists(_filePath))
            {
                await DisplayAlert("Error", "No se encontró el origen de datos de usuarios.", "OK");
                return;
            }

            string jsonString = File.ReadAllText(_filePath);
            var listaUsuarios = JsonSerializer.Deserialize<List<Usuario>>(jsonString) ?? new List<Usuario>();

            string passwordEncriptada = EncriptarContrasena(passwordInput);

            var usuarioValido = listaUsuarios.FirstOrDefault(u =>
                u.Username.Equals(usernameInput, StringComparison.OrdinalIgnoreCase) &&
                u.Password == passwordEncriptada);

            if (usuarioValido != null)
            {
                UsuarioService.UsuarioActual = usuarioValido;

                if (usuarioValido.IsAdmin)
                {
                    await DisplayAlert("¡Bienvenido Panel de Control!", $"Administrador: {usuarioValido.Username}", "Ingresar");
                    await Navigation.PushAsync(new AdminMainPage(usuarioValido.Username));
                    Navigation.RemovePage(this);
                }
                else
                {
                    await DisplayAlert("¡Bienvenido de vuelta!", $"Hola {usuarioValido.Username}, que pases una excelente lectura.", "Entrar");
                    await Navigation.PushAsync(new UserMainPage(usuarioValido.Username));
                    Navigation.RemovePage(this);
                }
            }
            else
            {
                await DisplayAlert("Error", "Usuario o contraseña incorrectos.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo verificar el usuario: {ex.Message}", "OK");
        }
    }

    private void OnTogglePasswordClicked(object sender, EventArgs e)
    {
        txtPassword.IsPassword = !txtPassword.IsPassword;
        btnTogglePassword.Text = txtPassword.IsPassword ? "👁️" : "🙈";
    }

    private async void OnRegisterTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegistroPage());
    }

    public static string EncriptarContrasena(string contrasena)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(contrasena));
        StringBuilder builder = new StringBuilder();
        foreach (byte b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }
        return builder.ToString();
    }
}
