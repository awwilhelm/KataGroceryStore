using System;
namespace katacheckoutordertotalapi.Models
{
    public class Item
    {
        public string ItemIdentifier { get; set; }

        public decimal? WeightInPounds { get; set; }

        public Item()
        {
        }
    }
}
