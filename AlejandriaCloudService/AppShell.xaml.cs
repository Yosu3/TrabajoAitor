using AlejandriaCloudService.Vista;

namespace AlejandriaCloudService;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("registro", typeof(RegistroPage));
        Routing.RegisterRoute("usuario", typeof(UserMainPage));
        Routing.RegisterRoute("admin", typeof(AdminMainPage));
        Routing.RegisterRoute("catalogo", typeof(CatalogoPage));
        Routing.RegisterRoute("prestamos", typeof(prestamosPage));
        Routing.RegisterRoute("admin-prestamos", typeof(AdminPrestamosPage));
        Routing.RegisterRoute("anadir-libro", typeof(AnadirLibroPage));
    }
}