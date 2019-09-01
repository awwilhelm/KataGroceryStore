using System;
namespace katacheckoutordertotalapi.Models
{
    public class PerUnitItem : Item
    {
        public decimal PricePerUnit { get; set; }

        public PerUnitItem()
        {
        }
    }
}
