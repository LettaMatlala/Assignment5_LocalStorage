using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Assignment5.Models
{
    [Table("shopping_cart")]
    public class ShoppingCart : BaseModel
    {
        [PrimaryKey("id", false)]
        [JsonProperty("id")]
        public int Id { get; set; }

        [Column("profile_id")]
        [JsonProperty("profile_id")]
        public Guid ProfileId { get; set; }

        [Column("item_id")]
        [JsonProperty("item_id")]
        public int ItemId { get; set; }

        [Column("quantity")]
        [JsonProperty("quantity")]
        public int Quantity { get; set; }
    }
}