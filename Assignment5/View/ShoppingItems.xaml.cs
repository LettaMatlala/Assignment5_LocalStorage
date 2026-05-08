using Assignment5.Models;
using Assignment5.ViewModel;

namespace Assignment5.View
{
    public partial class ShoppingItems : ContentPage
    {
        private readonly ShoppingItemsViewModel _vm;
        private ShoppingItem? _selectedItem;

        public ShoppingItems()
        {
            InitializeComponent();
            _vm = new ShoppingItemsViewModel();
            BindingContext = _vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _vm.InitAsync();
        }

        // ✅ When user taps a row, store the selected item
        private void OnItemSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is ShoppingItem item)
            {
                _selectedItem = item;
                System.Diagnostics.Debug.WriteLine($"[Selection] Selected: {item.Name}, Id: {item.Id}");
            }
        }

        // ✅ When Add to Cart is clicked, use the stored selected item
        private async void OnAddToCartClicked(object sender, EventArgs e)
        {
            try
            {
                // ✅ Try to get item from button's visual tree first
                ShoppingItem? item = null;

                if (sender is Button btn)
                {
                    // Walk up visual tree
                    Element? current = btn;
                    while (current != null)
                    {
                        if (current.BindingContext is ShoppingItem found)
                        {
                            item = found;
                            break;
                        }
                        current = current.Parent;
                    }
                }

                // ✅ Fallback to selected item
                if (item == null)
                    item = _selectedItem;

                // ✅ Fallback to first item in list (last resort)
                if (item == null && _vm.Items.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine("[AddToCart] WARNING: Using fallback item selection");
                    // Don't add - show error instead
                    await DisplayAlert("Please select", "Please tap on an item first, then click Add to Cart.", "OK");
                    return;
                }

                if (item == null)
                {
                    await DisplayAlert("Error", "No item selected.", "OK");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[AddToCart] Adding: {item.Name}, Id: {item.Id}");

                if (sender is Button b) b.IsEnabled = false;
                bool success = await _vm.AddToCartAsync(item.Id);
                if (sender is Button b2) b2.IsEnabled = true;

                System.Diagnostics.Debug.WriteLine($"[AddToCart] Result: {success}");

                // ✅ Clear selection after adding
                ItemsCollection.SelectedItem = null;
                _selectedItem = null;

                if (success)
                    await Shell.Current.GoToAsync(nameof(ShoppingCart));
                else
                    await DisplayAlert("Failed", "Could not add item to cart.", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AddToCart Exception] {ex.Message}");
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void OnViewCartClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(ShoppingCart));
        }
    }
}