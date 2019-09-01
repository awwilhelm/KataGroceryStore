using System;
using System.Threading.Tasks;
using katacheckoutordertotalapi.Models;

namespace katacheckoutordertotalapi.BLL
{
    public class GroceryRegisterBll
    {
        public GroceryRegisterBll()
        {
        }

        public Task<bool> AddItem(Item item)
        {
            return Task.Run(() => { return false; });
        }

        public Task<bool> RemoveItem(Item item)
        {
            return Task.Run(() => { return false; });
        }
    }
}
