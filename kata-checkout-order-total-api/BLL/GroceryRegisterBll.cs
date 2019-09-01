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

        private async Task RecalculateRegisterValue()
        {
            await Task.Run(() => { 
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
            });
        }
    }
}
