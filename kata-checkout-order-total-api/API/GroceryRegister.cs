using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using katacheckoutordertotalapi.BLL;
using katacheckoutordertotalapi.Models;

namespace katacheckoutordertotalapi.API
{
    public class GroceryRegister
    {
        public GroceryRegisterBll groceryRegisterBll;
        public GroceryRegister()
        {
            groceryRegisterBll = new GroceryRegisterBll();
        }

        public async Task<bool> ScanItem(Item item)
        {
            return await groceryRegisterBll.AddItem(item);
        }

        public async Task<bool> RemoveItem(Item item)
        {
            return await Task.Run(() => { return false; });
        }

        public async Task<List<Item>> GetRegisterLineItems()
        {
            return await groceryRegisterBll.GetRegisterLineItems();
        }
    }
}
