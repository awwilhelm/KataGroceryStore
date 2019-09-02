using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using katacheckoutordertotalapi.DATA;
using katacheckoutordertotalapi.Models;

namespace katacheckoutordertotalapi.BLL
{
    public class GroceryRegisterBll
    {
        public Costs costs;
        public GroceryRegisterBll()
        {
            costs = new Costs();
        }

        public async Task<bool> AddItem(Item item)
        {
            if(!costs.UniqueStoreItems.ContainsKey(item.ItemIdentifier))
            {
                return false;
            }

            if(costs.UniqueStoreItems[item.ItemIdentifier].StoreItemPriceType == StoreItemPriceType.perPound && item.WeightInPounds == null)
            {
                return false;
            }


            costs.CurrentRegisterLineItems.Add(item);

            await RecalculateRegisterValue();

            return true;
        }

        public async Task<bool> RemoveItem(Item item)
        {
            if (!costs.CurrentRegisterLineItems.Any(x => x.ItemIdentifier == item.ItemIdentifier))
            {
                return false;
            }

            if(item.WeightInPounds != null && !costs.CurrentRegisterLineItems.Any(x => x.ItemIdentifier == item.ItemIdentifier
                && x.WeightInPounds == item.WeightInPounds))
            {
                return false;
            }

            var firstOccurenceIndex = costs.CurrentRegisterLineItems.IndexOf(item);
            costs.CurrentRegisterLineItems.RemoveAt(firstOccurenceIndex);

            await RecalculateRegisterValue();

            return true;
        }

        public async Task<List<Item>> GetRegisterLineItems()
        {
            return costs.CurrentRegisterLineItems;
        }

        public async Task RecalculateRegisterValue()
        {
            decimal newCost = 0M;
            foreach(var item in costs.CurrentRegisterLineItems)
            {
                var storeItem = costs.UniqueStoreItems[item.ItemIdentifier];
                if(storeItem.StoreItemPriceType == StoreItemPriceType.perItem)
                {
                    newCost += storeItem.Price;
                } else if(storeItem.StoreItemPriceType == StoreItemPriceType.perPound)
                {
                    newCost += (storeItem.Price * item.WeightInPounds ?? 0);
                }
            }
            costs.RegisterValueWithoutDiscount = newCost;
        }

        public async Task CalculateDiscount()
        {
            decimal newDiscount = 0M;
            foreach(var discount in costs.ItemDiscounts)
            {
                var lineItemsForDiscount = costs.CurrentRegisterLineItems
                    .Where(x => x.ItemIdentifier == discount.ItemIdentifier).ToList();

                if (lineItemsForDiscount.Count > 0)
                {
                    var storeItem = costs.UniqueStoreItems[discount.ItemIdentifier];
                    int numberOfDiscountReuse = 0;

                    if(storeItem.StoreItemPriceType == StoreItemPriceType.perItem)
                    {
                        numberOfDiscountReuse = (int)(lineItemsForDiscount.Count / discount.QuantityForDeal);
                    } else if(storeItem.StoreItemPriceType == StoreItemPriceType.perPound)
                    {
                        numberOfDiscountReuse = (int)(lineItemsForDiscount
                            .Where(x=>x.ItemIdentifier == discount.ItemIdentifier)
                            .Sum(x=>x.WeightInPounds) / discount.QuantityForDeal);
                    }

                    var numberOfUsesWithLimit = -1;

                    if (discount.QuantityLimit != null)
                    {
                        numberOfUsesWithLimit = (int)((discount.QuantityLimit ?? 1) / discount.QuantityForDeal);
                    }

                    if (numberOfUsesWithLimit != -1 && numberOfDiscountReuse > numberOfUsesWithLimit)
                    {
                        numberOfDiscountReuse = numberOfUsesWithLimit;
                    }

                    if (discount.PriceModifier != null)
                    {
                        newDiscount += (discount.PriceModifier ?? 0) * storeItem.Price * numberOfDiscountReuse;
                    }
                    else if (discount.FlatRate != null)
                    {
                        var priceOfSavings = storeItem.Price * discount.QuantityForDeal - discount.FlatRate;
                        newDiscount += (priceOfSavings ?? 0) * numberOfDiscountReuse;
                    }
                }
            }
            costs.DiscountValue = newDiscount;
        }
    }
}
