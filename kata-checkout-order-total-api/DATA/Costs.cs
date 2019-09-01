using System;
using System.Collections.Generic;

namespace katacheckoutordertotalapi.DATA
{
    public class Costs
    {
        public static decimal TotalRegisterValue
        { 
            get
            {
                return RegisterValueWithoutDiscount - DiscountValue;
            }
        }
        private static decimal RegisterValueWithoutDiscount { get; set; }
        private static decimal DiscountValue { get; set; }

        public Dictionary<string, StoreItem> UniqueStoreItems = new Dictionary<string, StoreItem>()
        {
            {
                "oreos", 
                new StoreItem
                {
                    StoreItemPriceType = StoreItemPriceType.perItem,
                    Price = 3.95M
                }
            },
            {
                "paper",
                new StoreItem
                {
                    StoreItemPriceType = StoreItemPriceType.perItem,
                    Price = 5.50M
                }
            },
            {
                "pen",
                new StoreItem
                {
                    StoreItemPriceType = StoreItemPriceType.perItem,
                    Price = 1.250M
                }
            },
            {
                "groundbeef",
                new StoreItem
                {
                    StoreItemPriceType = StoreItemPriceType.perPound,
                    Price = 5.95M
                }
            },
            {
                "banana",
                new StoreItem
                {
                    StoreItemPriceType = StoreItemPriceType.perPound,
                    Price = 3.95M
                }
            },
        };

    }

    public enum StoreItemPriceType
    {
        perItem,
        perPound
    }

    public class StoreItem
    {
        public StoreItemPriceType StoreItemPriceType { get; set; }

        public decimal Price { get; set; }
    }
}
