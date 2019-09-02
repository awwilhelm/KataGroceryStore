using System;
using System.Collections.Generic;
using katacheckoutordertotalapi.Models;

namespace katacheckoutordertotalapi.DATA
{
    public class Costs
    {
        public decimal TotalRegisterValue
        { 
            get
            {
                return RegisterValueWithoutDiscount - DiscountValue;
            }
        }
        public List<Item> CurrentRegisterLineItems { get; set; } = new List<Item>();
        public decimal RegisterValueWithoutDiscount { get; set; }
        public decimal DiscountValue { get; set; }

        public List<ItemDiscount> ItemDiscounts = new List<ItemDiscount>()
        {
            new ItemDiscount()
            {
                ItemIdentifier = "oreos",
                QuantityForDeal = 3,
                PriceModifier = 1
            },
            new ItemDiscount()
            {
                ItemIdentifier = "banana",
                QuantityForDeal = 3,
                PriceModifier = 0.5M
            },
            new ItemDiscount()
            {
                ItemIdentifier = "pen",
                QuantityForDeal = 2,
                PriceModifier = 1,
                QuantityLimit = 4
            },
            new ItemDiscount()
            {
                ItemIdentifier = "belt",
                QuantityForDeal = 2,
                FlatRate = 15M
            }
        };

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
                "belt",
                new StoreItem
                {
                    StoreItemPriceType = StoreItemPriceType.perItem,
                    Price = 10M
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

    public class ItemDiscount
    {
        public string ItemIdentifier { get; set; }
        public int QuantityForDeal { get; set; }
        public decimal? PriceModifier { get; set; }
        public decimal? FlatRate { get; set; }
        public int? QuantityLimit { get; set; }
    }
}
