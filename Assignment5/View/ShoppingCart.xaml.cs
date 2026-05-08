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

        // Load cart data when the page appears
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _vm.InitAsync();
        }

        // Handle the Remove button click event
        private async void OnRemoveClicked(object sender, EventArgs e)
        {
            if (sender is not Button btn) return;

            if (btn.BindingContext is not CartItemDisplay cartItem)
            {
                Console.WriteLine($"[RemoveClicked] BindingContext type: {btn.BindingContext?.GetType().Name ?? "NULL"}");
                return;
            }

            Console.WriteLine($"[RemoveClicked] Removing: {cartItem.Name}, id={cartItem.Id}");
            await _vm.RemoveFromCartAsync(cartItem.Id);  // ✅ int
        }

        // Handle the Clear Cart button click event
        private async void OnClearCartClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Clear Cart", "Remove all items from cart?", "Yes", "No");
            if (confirm)
                await _vm.ClearCartAsync();
        }

        // Handle the Back to Shopping button click event
        private async void OnBackToShoppingClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//ShoppingItems");
        }

        // Handle the View Profile button click event
        private async void OnViewProfileClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}