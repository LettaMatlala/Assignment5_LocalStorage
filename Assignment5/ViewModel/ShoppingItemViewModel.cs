using Assignment5.Models;
using Assignment5.Services;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;

namespace Assignment5.ViewModel
{
    public class ShoppingItemsViewModel : BaseViewModel
    {
        private readonly SupabaseService _service;
        private readonly Guid _profileId;

        public ObservableCollection<ShoppingItem> Items { get; } = new();

        public ShoppingItemsViewModel()
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

            Console.WriteLine($"[ShoppingItemsVM] Using profileId: {_profileId}");
        }

        public async Task InitAsync()
        {
            IsBusy = true;
            try
            {
                Items.Clear();
                var items = await _service.GetShoppingItemsAsync();
                Console.WriteLine($"[InitAsync] Loaded {items.Count} items");
                foreach (var item in items)
                    Items.Add(item);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoadItems Error] {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        
        public async Task<bool> AddToCartAsync(int itemId)
        {
            Console.WriteLine($"[AddToCartAsync] profileId={_profileId}, itemId={itemId}");
            try
            {
                return await _service.AddToCartAsync(_profileId, itemId, 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AddToCart Error] {ex.Message}");
                return false;
            }
        }
    }
}