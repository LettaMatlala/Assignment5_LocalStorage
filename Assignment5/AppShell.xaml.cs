using Microsoft.Maui.Controls;
using Assignment5.View;

namespace Assignment5
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes for pages not in the Shell hierarchy 
            Routing.RegisterRoute(nameof(ShoppingItems), typeof(ShoppingItems));
            Routing.RegisterRoute(nameof(ShoppingCart), typeof(ShoppingCart));
        }
    }
}
