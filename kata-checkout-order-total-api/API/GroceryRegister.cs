using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using katacheckoutordertotalapi.Models;

namespace katacheckoutordertotalapi.API
{
    public class GroceryRegister
    {
        public GroceryRegister()
        {
        }

        public Task<bool> ScanItem(string itemIdentifier, decimal weight = -1)
        {
            return Task.Run(()=> { return false; });
        }


        public Task<bool> RemoveItem(string itemIdentifier, decimal weight = -1)
        {
            return Task.Run(() => { return false; });
        }

        public Task<List<Item>> GetRegisterLineItems()
        {
            return Task.Run(() => { return new List<Item>(); }); 
        }
    }
}
