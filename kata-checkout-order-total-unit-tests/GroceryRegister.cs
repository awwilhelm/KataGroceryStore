using System;
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
            var itemToScan = new Item() { ItemIdentifier = "oreos" };

            Assert.IsTrue((await groceryRegister.GetRegisterLineItems()).Count == 0);
            var isScanSuccessful = await groceryRegister.ScanItem(itemToScan);
            Assert.IsTrue(isScanSuccessful);
            Assert.IsTrue((await groceryRegister.GetRegisterLineItems()).Count == 1);
            Assert.IsTrue((await groceryRegister.GetRegisterLineItems()).Where(x => x.ItemIdentifier == itemToScan.ItemIdentifier)
                .ToList()
                .Count == 1);
        }

        [Test]
        public async Task AcceptAScannedItemWithWeight()
        {
            var itemToScan = new Item() { ItemIdentifier = "groundbeef", WeightInPounds = 1.34M };
            Console.WriteLine("hello");
            Console.WriteLine(await groceryRegister.GetRegisterLineItems());

            Assert.IsTrue((await groceryRegister.GetRegisterLineItems()).Count == 0);
            var isScanSuccessful = await groceryRegister.ScanItem(itemToScan);
            Assert.IsTrue(isScanSuccessful);
            Assert.IsTrue((await groceryRegister.GetRegisterLineItems()).Count == 1);
            Assert.IsTrue((await groceryRegister.GetRegisterLineItems())
                .Where(x => x.ItemIdentifier == itemToScan.ItemIdentifier && x.WeightInPounds == itemToScan.WeightInPounds)
                .ToList()
                .Count == 1);
        }

        [Test]
        public async Task RemoveAScannedItem()
        {
            var itemToScan = new Item() { ItemIdentifier = "oreos" };
            await groceryRegister.ScanItem(itemToScan);
            var currentLineItems = await groceryRegister.GetRegisterLineItems();

            Assert.IsTrue(currentLineItems.Count == 1);
            var numberOfDuplicates = currentLineItems
                .Where(x => x.ItemIdentifier == itemToScan.ItemIdentifier)
                .ToList()
                .Count;
            var isRemovalSuccessful = await groceryRegister.RemoveItem(itemToScan);
            Assert.IsTrue(isRemovalSuccessful);
            Assert.IsTrue((await groceryRegister.GetRegisterLineItems()).Count == 0);
            Assert.IsTrue(currentLineItems
                .Where(x => x.ItemIdentifier == itemToScan.ItemIdentifier)
                .ToList()
                .Count == numberOfDuplicates - 1);
        }

        [Test]
        public async Task RemoveAScannedItemWithWeight()
        {
            var itemToScan = new Item() { ItemIdentifier = "groundbeef", WeightInPounds = 1.34M };
            await groceryRegister.ScanItem(itemToScan);

            var currentLineItems = await groceryRegister.GetRegisterLineItems();
            Assert.IsTrue((currentLineItems).Count == 1);
            var numberOfDuplicates = currentLineItems
                .Where(x => x.ItemIdentifier == itemToScan.ItemIdentifier && x.WeightInPounds == itemToScan.WeightInPounds)
                .ToList()
                .Count;
            var isRemovalSuccessful = await groceryRegister.RemoveItem(itemToScan);
            Assert.IsTrue(isRemovalSuccessful);
            currentLineItems = await groceryRegister.GetRegisterLineItems();
            Assert.IsTrue((currentLineItems).Count == 0);
            Assert.IsTrue(currentLineItems
                .Where(x => x.ItemIdentifier == itemToScan.ItemIdentifier && x.WeightInPounds == itemToScan.WeightInPounds)
                .ToList()
                .Count == numberOfDuplicates - 1);
        }
    }
}