using Assignment5.ViewModel;

namespace Assignment5.View
{
    public partial class ShoppingCart : ContentPage
    {
        private readonly ShoppingCartViewModel _vm;

        public ShoppingCart()
        {
            InitializeComponent();
            _vm = new ShoppingCartViewModel();
            BindingContext = _vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _vm.InitAsync();
        }

        private async void OnRemoveClicked(object sender, EventArgs e)
        {
            if (sender is not Button btn) return;

            if (btn.BindingContext is not CartItemDisplay cartItem)
            {
                Console.WriteLine($"[RemoveClicked] BindingContext type: {btn.BindingContext?.GetType().Name ?? "NULL"}");
                return;
            }

            Console.WriteLine($"[RemoveClicked] Removing: {cartItem.Name}, id={cartItem.Id}");
            await _vm.RemoveFromCartAsync(cartItem.Id);  
        }

        private async void OnClearCartClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Clear Cart", "Remove all items from cart?", "Yes", "No");
            if (confirm)
                await _vm.ClearCartAsync();
        }

        private async void OnBackToShoppingClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//ShoppingItems");
        }

        private async void OnViewProfileClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}