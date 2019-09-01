using System;
namespace katacheckoutordertotalapi.Models
{
    public class PerWeightItem : Item
    {
        public decimal PricePerPound { get; set; }

        public PerWeightItem()
        {
        }
    }
}
