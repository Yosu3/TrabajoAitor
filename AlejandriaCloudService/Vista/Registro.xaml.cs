using AlejandriaCloudService.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AlejandriaCloudService.Vista;

public partial class RegistroPage : ContentPage
{
    private readonly string _filePath;

    public RegistroPage()
    {
        InitializeComponent();
        _filePath = Path.Combine(FileSystem.AppDataDirectory, "Usuarios.json");
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
            string.IsNullOrWhiteSpace(txtApellido.Text) ||
            string.IsNullOrWhiteSpace(txtDni.Text) ||
            string.IsNullOrWhiteSpace(txtEmail.Text) ||
            string.IsNullOrWhiteSpace(txtPassword.Text))
        {
            await DisplayAlert("Campos Vacíos", "Por favor, rellene todos los campos para continuar.", "OK");
            return;
        }

        string usernameNuevo = txtUsername.Text.Trim();
        string apellidoNuevo = txtApellido.Text.Trim();
        string dniNuevo = txtDni.Text.Trim().ToUpper();
        string emailNuevo = txtEmail.Text.Trim().ToLower();
        string passwordNueva = txtPassword.Text;

        if (!Regex.IsMatch(usernameNuevo, @"^[\p{L}][\p{L}0-9_]{3,19}$"))
        {
            await DisplayAlert("Usuario no válido", "El usuario debe tener entre 4 y 20 caracteres, empezar por una letra y solo contener letras, números o guion bajo.", "OK");
            return;
        }

        if (!Regex.IsMatch(apellidoNuevo, @"^[\p{L}][\p{L}\s'’-]{1,49}$"))
        {
            await DisplayAlert("Apellido no válido", "El apellido solo puede contener letras, espacios, apóstrofes o guiones.", "OK");
            return;
        }

        if (!Regex.IsMatch(emailNuevo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            await DisplayAlert("Formato Incorrecto", "El formato del correo electrónico no es válido.", "OK");
            return;
        }

        if (!ValidarDniEspañol(dniNuevo))
        {
            await DisplayAlert("DNI Inválido", "El DNI introducido no es un documento válido español.", "OK");
            return;
        }

        if (!Regex.IsMatch(passwordNueva, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$"))
        {
            await DisplayAlert("Contraseña Débil", "La contraseña debe tener al menos 8 caracteres, incluir una mayúscula, una minúscula, un número y un carácter especial.", "OK");
            return;
        }

        try
        {
            List<Usuario> listaUsuarios = new List<Usuario>();
            if (File.Exists(_filePath))
            {
                string jsonStringExistente = File.ReadAllText(_filePath);
                listaUsuarios = JsonSerializer.Deserialize<List<Usuario>>(jsonStringExistente) ?? new List<Usuario>();
            }

            if (listaUsuarios.Any(u => u.Dni.Equals(dniNuevo, StringComparison.OrdinalIgnoreCase)))
            {
                await DisplayAlert("Registro Fallido", "El DNI ingresado ya está asociado a otra cuenta.", "OK");
                return;
            }

            if (listaUsuarios.Any(u => u.Email.Equals(emailNuevo, StringComparison.OrdinalIgnoreCase)))
            {
                await DisplayAlert("Registro Fallido", "El correo electrónico ya se encuentra registrado.", "OK");
                return;
            }

            if (listaUsuarios.Any(u => u.Username.Equals(usernameNuevo, StringComparison.OrdinalIgnoreCase)))
            {
                await DisplayAlert("Registro Fallido", "El nombre de usuario ya está registrado.", "OK");
                return;
            }

            Usuario nuevoUsuario = new Usuario
            {
                Username = usernameNuevo,
                Apellido = apellidoNuevo,
                Dni = dniNuevo,
                Email = emailNuevo,
                // CORRECCIÓN: Se cambió 'Login' por 'LoginPage'
                Password = LoginPage.EncriptarContrasena(passwordNueva),
                IsAdmin = false
            };

            listaUsuarios.Add(nuevoUsuario);
            string jsonActualizado = JsonSerializer.Serialize(listaUsuarios, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, jsonActualizado);

            await DisplayAlert("Éxito", "Usuario registrado correctamente.", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo completar el registro: {ex.Message}", "OK");
        }
    }

    private bool ValidarDniEspañol(string dni)
    {
        if (dni.Length != 9) return false;

        string cadenaDni = dni;
        if (cadenaDni.StartsWith("X")) cadenaDni = "0" + cadenaDni.Substring(1);
        else if (cadenaDni.StartsWith("Y")) cadenaDni = "1" + cadenaDni.Substring(1);
        else if (cadenaDni.StartsWith("Z")) cadenaDni = "2" + cadenaDni.Substring(1);

        if (!Regex.IsMatch(cadenaDni, @"^[0-9]{8}[A-Z]$")) return false;

        string numeros = cadenaDni.Substring(0, 8);
        // CORRECCIÓN: Se extrae el char en la posición de la letra en lugar de asignar el string completo
        char letra = dni[8];

        if (!int.TryParse(numeros, out int numeroDni)) return false;

        string letrasValidas = "TRWAGMYFPDXBNJZSQVHLCKE";
        char letraCalculada = letrasValidas[numeroDni % 23];

        return letra == letraCalculada;
    }

    private async void OnLoginTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
