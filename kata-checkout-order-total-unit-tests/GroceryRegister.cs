using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using katacheckoutordertotalapi.API;
using katacheckoutordertotalapi.BLL;
using katacheckoutordertotalapi.DATA;
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

        [Test]
        public async Task VerifyPricingStaysUpdated()
        {
            var groceryRegisterBll = groceryRegister.groceryRegisterBll;
            var costs = groceryRegisterBll.costs;
            var itemToScanOreos = new Item() { ItemIdentifier = "oreos" };
            var itemToScanPaper = new Item() { ItemIdentifier = "paper" };
            var itemToScanGroundBeef = new Item() { ItemIdentifier = "groundbeef", WeightInPounds = 2.3M };

            var oreoStorePrice = costs.UniqueStoreItems[itemToScanOreos.ItemIdentifier].Price;
            var paperStorePrice = costs.UniqueStoreItems[itemToScanPaper.ItemIdentifier].Price;
            var groundBeefStorePrice = costs.UniqueStoreItems[itemToScanGroundBeef.ItemIdentifier].Price;

            costs.CurrentRegisterLineItems.Add(itemToScanOreos);
            costs.CurrentRegisterLineItems.Add(itemToScanPaper);
            await groceryRegisterBll.RecalculateRegisterValue();
            Assert.IsTrue(costs.RegisterValueWithoutDiscount == oreoStorePrice + paperStorePrice);
            costs.CurrentRegisterLineItems.Add(itemToScanGroundBeef);
            await groceryRegisterBll.RecalculateRegisterValue();
            Assert.IsTrue(costs.RegisterValueWithoutDiscount == (oreoStorePrice + paperStorePrice)
                + (itemToScanGroundBeef.WeightInPounds * groundBeefStorePrice));
        }

        [Test]
        public async Task OneDiscountActive()
        {
            var groceryRegisterBll = groceryRegister.groceryRegisterBll;
            var costs = groceryRegisterBll.costs;
            var itemToScanPen = new Item() { ItemIdentifier = "pen" };
            var penStorePrice = costs.UniqueStoreItems[itemToScanPen.ItemIdentifier].Price;
            var itemDiscount = costs.ItemDiscounts.Where(x => x.ItemIdentifier == itemToScanPen.ItemIdentifier)
                .FirstOrDefault();

            for(var i = 0; i < itemDiscount.QuantityForDeal - 1; i++)
            {
                costs.CurrentRegisterLineItems.Add(itemToScanPen);
                await groceryRegisterBll.CalculateDiscount();
                Assert.IsTrue(costs.DiscountValue == 0);
            }
            costs.CurrentRegisterLineItems.Add(itemToScanPen);
            await groceryRegisterBll.CalculateDiscount();
            Assert.IsTrue(costs.DiscountValue == itemDiscount.PriceModifier * penStorePrice);
        }

        [Test]
        public async Task MultipleDiscountActive()
        {
            var groceryRegisterBll = groceryRegister.groceryRegisterBll;
            var costs = groceryRegisterBll.costs;
            var itemToScanPen = new Item() { ItemIdentifier = "pen" };
            var itemToScanOreos = new Item() { ItemIdentifier = "oreos" };
            var penStorePrice = costs.UniqueStoreItems[itemToScanPen.ItemIdentifier].Price;
            var oreoStorePrice = costs.UniqueStoreItems[itemToScanOreos.ItemIdentifier].Price;
            var itemDiscountPen = costs.ItemDiscounts.Where(x => x.ItemIdentifier == itemToScanPen.ItemIdentifier)
                .FirstOrDefault();
            var itemDiscountOreos = costs.ItemDiscounts.Where(x => x.ItemIdentifier == itemToScanOreos.ItemIdentifier)
                .FirstOrDefault();

            for (var i = 0; i < itemDiscountPen.QuantityForDeal - 1; i++)
            {
                costs.CurrentRegisterLineItems.Add(itemToScanPen);
                await groceryRegisterBll.CalculateDiscount();
                Assert.IsTrue(costs.DiscountValue == 0);
            }
            for (var i = 0; i < itemDiscountOreos.QuantityForDeal - 1; i++)
            {
                costs.CurrentRegisterLineItems.Add(itemToScanOreos);
                await groceryRegisterBll.CalculateDiscount();
                Assert.IsTrue(costs.DiscountValue == 0);
            }
            costs.CurrentRegisterLineItems.Add(itemToScanPen);
            costs.CurrentRegisterLineItems.Add(itemToScanOreos);

            await groceryRegisterBll.CalculateDiscount();
            Assert.IsTrue(costs.DiscountValue == (itemDiscountPen.PriceModifier * penStorePrice)
                + (itemDiscountOreos.PriceModifier * oreoStorePrice));
        }

        [Test]
        public async Task DiscountIsActiveUntilRemoveProduct()
        {
            var groceryRegisterBll = groceryRegister.groceryRegisterBll;
            var costs = groceryRegisterBll.costs;
            var itemToScanPen = new Item() { ItemIdentifier = "pen" };
            var penStorePrice = costs.UniqueStoreItems[itemToScanPen.ItemIdentifier].Price;
            var itemDiscount = costs.ItemDiscounts.Where(x => x.ItemIdentifier == itemToScanPen.ItemIdentifier)
                .FirstOrDefault();

            for (var i = 0; i < itemDiscount.QuantityForDeal - 1; i++)
            {
                costs.CurrentRegisterLineItems.Add(itemToScanPen);
                await groceryRegisterBll.CalculateDiscount();
                Assert.IsTrue(costs.DiscountValue == 0);
            }
            costs.CurrentRegisterLineItems.Add(itemToScanPen);
            await groceryRegisterBll.CalculateDiscount();
            Assert.IsTrue(costs.DiscountValue == itemDiscount.PriceModifier * penStorePrice);
            costs.CurrentRegisterLineItems.Remove(itemToScanPen);
            await groceryRegisterBll.CalculateDiscount();
            Assert.IsTrue(costs.DiscountValue == 0);
        }

        [Test]
        public async Task BuyNForXDollarsDiscount()
        {
            var groceryRegisterBll = groceryRegister.groceryRegisterBll;
            var costs = groceryRegisterBll.costs;
            var itemToScanBelt = new Item() { ItemIdentifier = "belt" };
            var beltStorePrice = costs.UniqueStoreItems[itemToScanBelt.ItemIdentifier].Price;
            var itemDiscount = costs.ItemDiscounts.Where(x => x.ItemIdentifier == itemToScanBelt.ItemIdentifier)
                .FirstOrDefault();

            for (var i = 0; i < itemDiscount.QuantityForDeal - 1; i++)
            {
                costs.CurrentRegisterLineItems.Add(itemToScanBelt);
                await groceryRegisterBll.CalculateDiscount();
                Assert.IsTrue(costs.DiscountValue == 0);
            }
            costs.CurrentRegisterLineItems.Add(itemToScanBelt);
            await groceryRegisterBll.CalculateDiscount();
            var priceOfSavings = beltStorePrice*itemDiscount.QuantityForDeal - itemDiscount.FlatRate;
            Assert.IsTrue(costs.DiscountValue == priceOfSavings);
        }

        [Test]
        public async Task OneDiscountWithLimit()
        {
            var groceryRegisterBll = groceryRegister.groceryRegisterBll;
            var costs = groceryRegisterBll.costs;
            var itemToScanPen = new Item() { ItemIdentifier = "pen" };
            var penStorePrice = costs.UniqueStoreItems[itemToScanPen.ItemIdentifier].Price;
            var itemDiscount = costs.ItemDiscounts.Where(x => x.ItemIdentifier == itemToScanPen.ItemIdentifier)
                .FirstOrDefault();

            for (var i = 0; i < itemDiscount.QuantityLimit - 1; i++)
            {
                costs.CurrentRegisterLineItems.Add(itemToScanPen);
                await groceryRegisterBll.CalculateDiscount();
            }
            costs.CurrentRegisterLineItems.Add(itemToScanPen);
            await groceryRegisterBll.CalculateDiscount();

            var numberOfDeals = (int)(itemDiscount.QuantityLimit / itemDiscount.QuantityForDeal);
            Assert.IsTrue(costs.DiscountValue == (itemDiscount.PriceModifier * penStorePrice) * numberOfDeals);

            for (var i = 0; i < itemDiscount.QuantityForDeal; i++)
            {
                costs.CurrentRegisterLineItems.Add(itemToScanPen);
            }
            await groceryRegisterBll.CalculateDiscount();
            Assert.IsTrue(costs.DiscountValue == (itemDiscount.PriceModifier * penStorePrice) * numberOfDeals);
        }

        [Test]
        public async Task PercentDiscountWithWeight()
        {
            var groceryRegisterBll = groceryRegister.groceryRegisterBll;
            var costs = groceryRegisterBll.costs;
            var itemToScanBanana = new Item() { ItemIdentifier = "banana", WeightInPounds = 0.9M };
            var bananaStorePrice = costs.UniqueStoreItems[itemToScanBanana.ItemIdentifier].Price;
            var itemDiscount = costs.ItemDiscounts.Where(x => x.ItemIdentifier == itemToScanBanana.ItemIdentifier)
                .FirstOrDefault();

            for (var i = 0; i < Decimal.Ceiling(itemDiscount.QuantityForDeal/(itemToScanBanana.WeightInPounds??0) - 1); i++)
            {
                costs.CurrentRegisterLineItems.Add(itemToScanBanana);
                await groceryRegisterBll.CalculateDiscount();
                Assert.IsTrue(costs.DiscountValue == 0);
            }
            costs.CurrentRegisterLineItems.Add(itemToScanBanana);
            await groceryRegisterBll.CalculateDiscount();

            Assert.IsTrue(costs.DiscountValue == bananaStorePrice * itemDiscount.PriceModifier);
        }
    }
}