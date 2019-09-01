using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using katacheckoutordertotalapi.API;
using katacheckoutordertotalapi.Models;
using Moq;
using NUnit.Framework;

namespace Tests
{
    public class GroceryRegisterTests
    {
        GroceryRegister groceryRegister;
        [SetUp]
        public void Setup()
        {
            groceryRegister = new GroceryRegister();
        }

        [Test]
        public async Task AcceptAScannedItem()
        {
            var itemName = "oreo";

            Assert.IsTrue((await groceryRegister.GetRegisterLineItems()).Count == 0);
            var isScanSuccessful = await groceryRegister.ScanItem(itemName);
            Assert.IsTrue(isScanSuccessful);
            Assert.IsTrue((await groceryRegister.GetRegisterLineItems()).Count == 1);
            Assert.IsTrue((await groceryRegister.GetRegisterLineItems()).Where(x => x.Name == itemName)
                .ToList()
                .Count == 1);
        }

        [Test]
        public async Task AcceptAScannedItemWithWeight()
        {
            var itemName = "groundbeef";
            var itemWeightInPounds = 1.34M;

            Assert.IsTrue((await groceryRegister.GetRegisterLineItems()).Count == 0);
            var isScanSuccessful = await groceryRegister.ScanItem(itemName, itemWeightInPounds);
            Assert.IsTrue(isScanSuccessful);
            Assert.IsTrue((await groceryRegister.GetRegisterLineItems()).Count == 1);
            Assert.IsTrue((await groceryRegister.GetRegisterLineItems())
                .Where(x => x.Name == itemName && x.WeightInPounds == itemWeightInPounds)
                .ToList()
                .Count == 1);
        }

        [Test]
        public async Task RemoveAScannedItem()
        {
            var itemName = "oreo";
            await groceryRegister.ScanItem(itemName);
            var currentLineItems = await groceryRegister.GetRegisterLineItems();

            Assert.IsTrue(currentLineItems.Count == 1);
            var numberOfDuplicates = currentLineItems
                .Where(x => x.Name == itemName)
                .ToList()
                .Count;
            var isRemovalSuccessful = await groceryRegister.RemoveItem(itemName);
            Assert.IsTrue(isRemovalSuccessful);
            Assert.IsTrue((await groceryRegister.GetRegisterLineItems()).Count == 0);
            Assert.IsTrue(currentLineItems
                .Where(x => x.Name == itemName)
                .ToList()
                .Count == numberOfDuplicates - 1);
        }

        [Test]
        public async Task RemoveAScannedItemWithWeight()
        {
            var itemName = "oreo";
            var itemWeightInPounds = 1.34M;
            await groceryRegister.ScanItem(itemName, itemWeightInPounds);

            var currentLineItems = await groceryRegister.GetRegisterLineItems();
            Assert.IsTrue((currentLineItems).Count == 1);
            var numberOfDuplicates = currentLineItems
                .Where(x => x.Name == itemName && x.WeightInPounds == itemWeightInPounds)
                .ToList()
                .Count;
            var isRemovalSuccessful = await groceryRegister.RemoveItem(itemName, itemWeightInPounds);
            Assert.IsTrue(isRemovalSuccessful);
            currentLineItems = await groceryRegister.GetRegisterLineItems();
            Assert.IsTrue((currentLineItems).Count == 0);
            Assert.IsFalse(currentLineItems
                .Where(x => x.Name == itemName && x.WeightInPounds == itemWeightInPounds)
                .ToList()
                .Count == numberOfDuplicates - 1);
        }
    }
}