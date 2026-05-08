using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Assignment5.Models
{
    [Table("shopping_items")]
    public class ShoppingItem : BaseModel
    {
        [PrimaryKey("item_id", false)]
        [JsonProperty("item_id")]  
        public int Id { get; set; }

        [Column("name")]
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [Column("price")]
        [JsonProperty("price")]
        public decimal Price { get; set; }

        [Column("imagelink")]
        [JsonProperty("imagelink")]
        public string ImageLink { get; set; } = string.Empty;

        [Column("quantity")]
        [JsonProperty("quantity")]
        public int Quantity { get; set; }
    }
}