using Assignment5.Models;
using Assignment5.Services;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;

namespace Assignment5.ViewModel
{
    public class CartItemDisplay
    {
        public int Id { get; set; }
        public int ItemId { get; set; }  // ✅ now int
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public string PriceDisplay => $"R{Price:N2} each";
        public string SubtotalDisplay => $"Subtotal: R{Price * Quantity:N2}";
        public string QuantityDisplay => $"Qty: {Quantity}";
    }

    public class ShoppingCartViewModel : BaseViewModel
    {
        private readonly SupabaseService _service;
        private readonly Guid _profileId;

        public ObservableCollection<CartItemDisplay> CartItems { get; } = new();

        public decimal CartTotal => CartItems.Sum(i => i.Price * i.Quantity);
        public string CartTotalDisplay => $"Total: R{CartTotal:N2}";

        public ShoppingCartViewModel()
        {
            _service = new SupabaseService();
            var stored = Preferences.Get("UserId", string.Empty);

            if (string.IsNullOrEmpty(stored))
            {
                var newId = Guid.NewGuid();
                Preferences.Set("UserId", newId.ToString());
                _profileId = newId;
            }
            else
            {
                _profileId = Guid.Parse(stored);
            }

            Console.WriteLine($"[ShoppingCartVM] Using profileId: {_profileId}");
        }

        public async Task InitAsync()
        {
            IsBusy = true;
            try
            {
                CartItems.Clear();
                var cartRows = await _service.GetCartAsync(_profileId);
                Console.WriteLine($"[CartVM] Loaded {cartRows.Count} cart rows");

                foreach (var row in cartRows)
                {
                    var item = await _service.GetShoppingItemByIdAsync(row.ItemId);
                    CartItems.Add(new CartItemDisplay
                    {
                        Id = row.Id,
                        ItemId = row.ItemId,
                        Name = item?.Name ?? $"Item #{row.ItemId}",
                        Description = item?.Description ?? string.Empty,
                        Price = item?.Price ?? 0,
                        Quantity = row.Quantity
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoadCart Error] {ex.Message}");
            }
            finally
            {
                IsBusy = false;
                RefreshTotal();
            }
        }

        public async Task RemoveFromCartAsync(int id)
        {
            try
            {
                var item = CartItems.FirstOrDefault(c => c.Id == id);
                if (item != null) CartItems.Remove(item);
                RefreshTotal();
                await _service.RemoveFromCartAsync(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RemoveFromCart Error] {ex.Message}");
            }
        }

        public async Task ClearCartAsync()
        {
            try
            {
                CartItems.Clear();
                RefreshTotal();
                await _service.ClearCartAsync(_profileId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ClearCart Error] {ex.Message}");
            }
        }

        private void RefreshTotal() => OnPropertyChanged(nameof(CartTotalDisplay));
    }
}