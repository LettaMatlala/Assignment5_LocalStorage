using Supabase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ShoppingCart = Assignment5.Models.ShoppingCart;
using UserProfile = Assignment5.Models.UserProfile;
using Assignment5.Models;
using static Supabase.Postgrest.Constants;

namespace Assignment5.Services
{
    public class SupabaseService
    {
        private readonly Client _client;
        private readonly HttpClient _http;
        private bool _initialized = false;

        private const string SupabaseUrl = "https://mprdfytvnmhrhvvwmjcf.supabase.co";
        private const string SupabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im1wcmRmeXR2bm1ocmh2dndtamNmIiwicm9sZSI6ImFub24iLCJpYXQiOjE3Nzc1MDExNDcsImV4cCI6MjA5MzA3NzE0N30.9rqQnxKWV9Jv7pAW3ElypP1R-mZpBC1D29SdQbw9rDs";
        private const string BucketName = "avatars";

        public SupabaseService()
        {
            var options = new SupabaseOptions { AutoConnectRealtime = false };
            _client = new Client(SupabaseUrl, SupabaseKey, options);

            _http = new HttpClient();
            _http.DefaultRequestHeaders.Add("apikey", SupabaseKey);
            _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {SupabaseKey}");
            _http.DefaultRequestHeaders.Add("Prefer", "return=representation");
        }

        private async Task EnsureInitializedAsync()
        {
            if (!_initialized)
            {
                await _client.InitializeAsync();
                _initialized = true;
            }
        }

        // ─── PROFILE ─────────────────────────────────────────────────────────────

        public async Task<UserProfile?> GetProfileByIdAsync(Guid userId)
        {
            await EnsureInitializedAsync();
            var response = await _client.From<UserProfile>()
                                        .Filter("id", Operator.Equals, userId.ToString())
                                        .Get();
            return response.Models.FirstOrDefault();
        }

        public async Task SaveProfileAsync(UserProfile profile)
        {
            await EnsureInitializedAsync();
            await _client.From<UserProfile>().Upsert(profile);
        }

        // ─── PROFILE PICTURE ─────────────────────────────────────────────────────

        public async Task<string?> UploadProfilePictureAsync(Guid userId, string localImagePath)
        {
            try
            {
                await EnsureInitializedAsync();
                string extension = Path.GetExtension(localImagePath).ToLower();
                string fileName = $"{userId}/avatar{extension}";
                string mimeType = extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    ".webp" => "image/webp",
                    _ => "application/octet-stream"
                };
                byte[] fileBytes = await File.ReadAllBytesAsync(localImagePath);
                try
                {
                    await _client.Storage.From(BucketName)
                                        .Remove(new List<string> { fileName });
                }
                catch { }
                await _client.Storage.From(BucketName).Upload(fileBytes, fileName,
                    new Supabase.Storage.FileOptions { ContentType = mimeType, Upsert = true });
                return _client.Storage.From(BucketName).GetPublicUrl(fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Upload Error] {ex.Message}");
                return null;
            }
        }

        // ─── SHOPPING ITEMS ──────────────────────────────────────────────────────

        public async Task<List<ShoppingItem>> GetShoppingItemsAsync()
        {
            try
            {
                var url = $"{SupabaseUrl}/rest/v1/shopping_items?select=*";
                Console.WriteLine($"[GetItems] URL: {url}");
                var res = await _http.GetStringAsync(url);
                Console.WriteLine($"[GetItems] Response: {res}");
                return JsonConvert.DeserializeObject<List<ShoppingItem>>(res) ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetItems Error] {ex.Message}");
                return new List<ShoppingItem>();
            }
        }

        // ✅ now takes int
        public async Task<ShoppingItem?> GetShoppingItemByIdAsync(int itemId)
        {
            try
            {
                var url = $"{SupabaseUrl}/rest/v1/shopping_items?item_id=eq.{itemId}&select=*";
                Console.WriteLine($"[GetItemById] URL: {url}");
                var res = await _http.GetStringAsync(url);
                Console.WriteLine($"[GetItemById] Response: {res}");
                var items = JsonConvert.DeserializeObject<List<ShoppingItem>>(res) ?? new();
                var item = items.FirstOrDefault();
                Console.WriteLine($"[GetItemById] Found: {item?.Name ?? "NULL"}");
                return item;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetItemById Error] {ex.Message}");
                return null;
            }
        }

        // ─── SHOPPING CART ───────────────────────────────────────────────────────

        public async Task<List<ShoppingCart>> GetCartAsync(Guid profileId)
        {
            try
            {
                var url = $"{SupabaseUrl}/rest/v1/shopping_cart?profile_id=eq.{profileId}&select=*";
                Console.WriteLine($"[GetCart] Fetching: {url}");
                var res = await _http.GetStringAsync(url);
                Console.WriteLine($"[GetCart] Response: {res}");
                var items = JsonConvert.DeserializeObject<List<ShoppingCart>>(res) ?? new();
                Console.WriteLine($"[GetCart] Parsed {items.Count} rows");
                return items;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetCart Error] {ex.Message}");
                return new List<ShoppingCart>();
            }
        }

        // ✅ now takes int itemId
        public async Task<bool> AddToCartAsync(Guid profileId, int itemId, int qty)
        {
            try
            {
                Console.WriteLine($"[AddToCart] Starting - profileId={profileId}, itemId={itemId}");

                // Step 1: Get stock item
                var stockItem = await GetShoppingItemByIdAsync(itemId);
                Console.WriteLine($"[AddToCart] stockItem={stockItem?.Name ?? "NULL"}, stock={stockItem?.Quantity}");
                if (stockItem == null)
                {
                    Console.WriteLine("[AddToCart] Stock item not found");
                    return false;
                }

                // Step 2: Get current cart
                var existingCart = await GetCartAsync(profileId);
                int alreadyInCart = existingCart
                    .Where(c => c.ItemId == itemId)
                    .Sum(c => c.Quantity);
                Console.WriteLine($"[AddToCart] alreadyInCart={alreadyInCart}, stockQty={stockItem.Quantity}");

                // Step 3: Stock check
                if (alreadyInCart + qty > stockItem.Quantity)
                {
                    Console.WriteLine("[AddToCart] Stock limit reached");
                    return false;
                }

                // Step 4: Update or insert
                var existing = existingCart.FirstOrDefault(c => c.ItemId == itemId);
                if (existing != null)
                {
                    Console.WriteLine($"[AddToCart] Updating existing row id={existing.Id}");
                    var patchUrl = $"{SupabaseUrl}/rest/v1/shopping_cart?id=eq.{existing.Id}";
                    var patchBody = JsonConvert.SerializeObject(new { quantity = existing.Quantity + qty });
                    var patchReq = new HttpRequestMessage(HttpMethod.Patch, patchUrl)
                    {
                        Content = new StringContent(patchBody, Encoding.UTF8, "application/json")
                    };
                    var patchRes = await _http.SendAsync(patchReq);
                    var patchText = await patchRes.Content.ReadAsStringAsync();
                    Console.WriteLine($"[AddToCart] PATCH status={patchRes.StatusCode}, body={patchText}");
                    return patchRes.IsSuccessStatusCode;
                }
                else
                {
                    Console.WriteLine("[AddToCart] Inserting new row");
                    var postUrl = $"{SupabaseUrl}/rest/v1/shopping_cart";
                    var postBody = JsonConvert.SerializeObject(new
                    {
                        profile_id = profileId,
                        item_id = itemId,
                        quantity = qty
                    });
                    Console.WriteLine($"[AddToCart] POST body={postBody}");
                    var postReq = new HttpRequestMessage(HttpMethod.Post, postUrl)
                    {
                        Content = new StringContent(postBody, Encoding.UTF8, "application/json")
                    };
                    var postRes = await _http.SendAsync(postReq);
                    var postText = await postRes.Content.ReadAsStringAsync();
                    Console.WriteLine($"[AddToCart] POST status={postRes.StatusCode}, body={postText}");
                    return postRes.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AddToCart Error] {ex.Message}");
                return false;
            }
        }

        public async Task RemoveFromCartAsync(int id)
        {
            try
            {
                var url = $"{SupabaseUrl}/rest/v1/shopping_cart?id=eq.{id}";
                var res = await _http.DeleteAsync(url);
                Console.WriteLine($"[RemoveFromCart] id={id}, status={res.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RemoveFromCart Error] {ex.Message}");
            }
        }

        public async Task ClearCartAsync(Guid profileId)
        {
            try
            {
                var url = $"{SupabaseUrl}/rest/v1/shopping_cart?profile_id=eq.{profileId}";
                var res = await _http.DeleteAsync(url);
                Console.WriteLine($"[ClearCart] profileId={profileId}, status={res.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ClearCart Error] {ex.Message}");
            }
        }
    }
}