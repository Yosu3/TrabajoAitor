namespace AlejandriaCloudService.Vista;

public partial class UserMainPage : ContentPage
{
    private bool isPasswordVisible = false;
    private Entry PasswordEntry;
    private Button TogglePasswordButton;
    public UserMainPage()
	{
		InitializeComponent();
	}

    private async void OnCatalogoTapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new CatalogoPage());
    }

    private async void OnPrestamosTapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new prestamosPage());
    }

	


}