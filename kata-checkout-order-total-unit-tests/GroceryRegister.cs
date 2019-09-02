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
        private readonly Item ItemToScanOreos = new Item() { ItemIdentifier = "oreos" };
        private readonly Item ItemToScanGroundBeef = new Item() { ItemIdentifier = "groundbeef", WeightInPounds = 1.34M };
        private readonly Item ItemToScanPaper = new Item() { ItemIdentifier = "paper" };
        private readonly Item ItemToScanPen = new Item() { ItemIdentifier = "pen" };
        private readonly Item ItemToScanBelt = new Item() { ItemIdentifier = "belt" };
        private readonly Item ItemToScanBanana = new Item() { ItemIdentifier = "banana", WeightInPounds = 0.9M };

        private GroceryRegisterBll GroceryRegisterBll;
        private Costs Costs;

        [SetUp]
        public void Setup()
        {
            groceryRegister = new GroceryRegister();
            GroceryRegisterBll = groceryRegister.groceryRegisterBll;
            Costs = GroceryRegisterBll.costs;
        }

        [Test]
        public async Task AcceptAScannedItem()
        {

            Assert.IsTrue((await groceryRegister.GetRegisterLineItems()).Count == 0);
            var isScanSuccessful = await groceryRegister.ScanItem(ItemToScanOreos);
            Assert.IsTrue(isScanSuccessful);
            Assert.IsTrue((await groceryRegister.GetRegisterLineItems()).Count == 1);
            Assert.IsTrue((await groceryRegister.GetRegisterLineItems()).Where(x => x.ItemIdentifier == ItemToScanOreos.ItemIdentifier)
                .ToList()
                .Count == 1);
        }

        [Test]
        public async Task AcceptAScannedItemWithWeight()
        {
            Assert.IsTrue((await groceryRegister.GetRegisterLineItems()).Count == 0);
            var isScanSuccessful = await groceryRegister.ScanItem(ItemToScanGroundBeef);
            Assert.IsTrue(isScanSuccessful);
            Assert.IsTrue((await groceryRegister.GetRegisterLineItems()).Count == 1);
            Assert.IsTrue((await groceryRegister.GetRegisterLineItems())
                .Where(x => x.ItemIdentifier == ItemToScanGroundBeef.ItemIdentifier && x.WeightInPounds == ItemToScanGroundBeef.WeightInPounds)
                .ToList()
                .Count == 1);
        }

        [Test]
        public async Task RemoveAScannedItem()
        {
            await groceryRegister.ScanItem(ItemToScanOreos);
            var currentLineItems = await groceryRegister.GetRegisterLineItems();

            Assert.IsTrue(currentLineItems.Count == 1);
            var numberOfDuplicates = currentLineItems
                .Where(x => x.ItemIdentifier == ItemToScanOreos.ItemIdentifier)
                .ToList()
                .Count;
            var isRemovalSuccessful = await groceryRegister.RemoveItem(ItemToScanOreos);
            Assert.IsTrue(isRemovalSuccessful);
            Assert.IsTrue((await groceryRegister.GetRegisterLineItems()).Count == 0);
            Assert.IsTrue(currentLineItems
                .Where(x => x.ItemIdentifier == ItemToScanOreos.ItemIdentifier)
                .ToList()
                .Count == numberOfDuplicates - 1);
        }

        [Test]
        public async Task RemoveAScannedItemWithWeight()
        {
            await groceryRegister.ScanItem(ItemToScanGroundBeef);

            var currentLineItems = await groceryRegister.GetRegisterLineItems();
            Assert.IsTrue((currentLineItems).Count == 1);
            var numberOfDuplicates = currentLineItems
                .Where(x => x.ItemIdentifier == ItemToScanGroundBeef.ItemIdentifier && x.WeightInPounds == ItemToScanGroundBeef.WeightInPounds)
                .ToList()
                .Count;
            var isRemovalSuccessful = await groceryRegister.RemoveItem(ItemToScanGroundBeef);
            Assert.IsTrue(isRemovalSuccessful);
            currentLineItems = await groceryRegister.GetRegisterLineItems();
            Assert.IsTrue((currentLineItems).Count == 0);
            Assert.IsTrue(currentLineItems
                .Where(x => x.ItemIdentifier == ItemToScanGroundBeef.ItemIdentifier && x.WeightInPounds == ItemToScanGroundBeef.WeightInPounds)
                .ToList()
                .Count == numberOfDuplicates - 1);
        }

        [Test]
        public async Task VerifyPricingStaysUpdated()
        {
            var oreoStorePrice = Costs.UniqueStoreItems[ItemToScanOreos.ItemIdentifier].Price;
            var paperStorePrice = Costs.UniqueStoreItems[ItemToScanPaper.ItemIdentifier].Price;
            var groundBeefStorePrice = Costs.UniqueStoreItems[ItemToScanGroundBeef.ItemIdentifier].Price;

            Costs.CurrentRegisterLineItems.Add(ItemToScanOreos);
            Costs.CurrentRegisterLineItems.Add(ItemToScanPaper);
            await GroceryRegisterBll.RecalculateRegisterValue();
            Assert.IsTrue(Costs.RegisterValueWithoutDiscount == oreoStorePrice + paperStorePrice);
            Costs.CurrentRegisterLineItems.Add(ItemToScanGroundBeef);
            await GroceryRegisterBll.RecalculateRegisterValue();
            Assert.IsTrue(Costs.RegisterValueWithoutDiscount == (oreoStorePrice + paperStorePrice)
                + (ItemToScanGroundBeef.WeightInPounds * groundBeefStorePrice));
        }

        [Test]
        public async Task OneDiscountActive()
        {
            var penStorePrice = Costs.UniqueStoreItems[ItemToScanPen.ItemIdentifier].Price;
            var itemDiscount = Costs.ItemDiscounts.Where(x => x.ItemIdentifier == ItemToScanPen.ItemIdentifier)
                .FirstOrDefault();

            await AddItemsToLineItems(ItemToScanPen, itemDiscount.QuantityForDeal - 1);
            Assert.IsTrue(Costs.DiscountValue == 0);

            await AddItemsToLineItems(ItemToScanPen, 1);
            Assert.IsTrue(Costs.DiscountValue == itemDiscount.PriceModifier * penStorePrice);
        }

        [Test]
        public async Task MultipleDiscountActive()
        {
            var penStorePrice = Costs.UniqueStoreItems[ItemToScanPen.ItemIdentifier].Price;
            var oreoStorePrice = Costs.UniqueStoreItems[ItemToScanOreos.ItemIdentifier].Price;
            var itemDiscountPen = Costs.ItemDiscounts.Where(x => x.ItemIdentifier == ItemToScanPen.ItemIdentifier)
                .FirstOrDefault();
            var itemDiscountOreos = Costs.ItemDiscounts.Where(x => x.ItemIdentifier == ItemToScanOreos.ItemIdentifier)
                .FirstOrDefault();

            await AddItemsToLineItems(ItemToScanPen, itemDiscountPen.QuantityForDeal - 1);
            Assert.IsTrue(Costs.DiscountValue == 0);

            await AddItemsToLineItems(ItemToScanOreos, itemDiscountOreos.QuantityForDeal - 1);
            Assert.IsTrue(Costs.DiscountValue == 0);

            await AddItemsToLineItems(ItemToScanPen, 1);
            await AddItemsToLineItems(ItemToScanOreos, 1);
            Assert.IsTrue(Costs.DiscountValue == (itemDiscountPen.PriceModifier * penStorePrice)
                + (itemDiscountOreos.PriceModifier * oreoStorePrice));
        }

        [Test]
        public async Task DiscountIsActiveUntilRemoveProduct()
        {
            var penStorePrice = Costs.UniqueStoreItems[ItemToScanPen.ItemIdentifier].Price;
            var itemDiscount = Costs.ItemDiscounts.Where(x => x.ItemIdentifier == ItemToScanPen.ItemIdentifier)
                .FirstOrDefault();

            await AddItemsToLineItems(ItemToScanPen, itemDiscount.QuantityForDeal - 1);
            Assert.IsTrue(Costs.DiscountValue == 0);

            await AddItemsToLineItems(ItemToScanPen, 1);
            Assert.IsTrue(Costs.DiscountValue == itemDiscount.PriceModifier * penStorePrice);

            Costs.CurrentRegisterLineItems.Remove(ItemToScanPen);
            await GroceryRegisterBll.CalculateDiscount();
            Assert.IsTrue(Costs.DiscountValue == 0);
        }

        [Test]
        public async Task BuyNForXDollarsDiscount()
        {
            var beltStorePrice = Costs.UniqueStoreItems[ItemToScanBelt.ItemIdentifier].Price;
            var itemDiscount = Costs.ItemDiscounts.Where(x => x.ItemIdentifier == ItemToScanBelt.ItemIdentifier)
                .FirstOrDefault();

            await AddItemsToLineItems(ItemToScanBelt, itemDiscount.QuantityForDeal - 1);
            Assert.IsTrue(Costs.DiscountValue == 0);
            await AddItemsToLineItems(ItemToScanBelt, 1);

            var priceOfSavings = beltStorePrice*itemDiscount.QuantityForDeal - itemDiscount.FlatRate;
            Assert.IsTrue(Costs.DiscountValue == priceOfSavings);
        }

        [Test]
        public async Task OneDiscountWithLimit()
        {
            var penStorePrice = Costs.UniqueStoreItems[ItemToScanPen.ItemIdentifier].Price;
            var itemDiscount = Costs.ItemDiscounts.Where(x => x.ItemIdentifier == ItemToScanPen.ItemIdentifier)
                .FirstOrDefault();

            await AddItemsToLineItems(ItemToScanPen, (itemDiscount.QuantityLimit??0));

            var numberOfDeals = (int)(itemDiscount.QuantityLimit / itemDiscount.QuantityForDeal);
            Assert.IsTrue(Costs.DiscountValue == (itemDiscount.PriceModifier * penStorePrice) * numberOfDeals);

            await AddItemsToLineItems(ItemToScanPen, itemDiscount.QuantityForDeal);
            Assert.IsTrue(Costs.DiscountValue == (itemDiscount.PriceModifier * penStorePrice) * numberOfDeals);
        }

        [Test]
        public async Task PercentDiscountWithWeight()
        {
            var bananaStorePrice = Costs.UniqueStoreItems[ItemToScanBanana.ItemIdentifier].Price;
            var itemDiscount = Costs.ItemDiscounts.Where(x => x.ItemIdentifier == ItemToScanBanana.ItemIdentifier)
                .FirstOrDefault();

            await AddItemsToLineItems(ItemToScanBanana,
                (int)Decimal.Ceiling(itemDiscount.QuantityForDeal / (ItemToScanBanana.WeightInPounds ?? 0)));

            Assert.IsTrue(Costs.DiscountValue == bananaStorePrice * itemDiscount.PriceModifier);
        }

        private async Task AddItemsToLineItems(Item item, int count)
        {
            for (var i = 0; i < count; i++)
            {
                Costs.CurrentRegisterLineItems.Add(item);
                await GroceryRegisterBll.CalculateDiscount();
            }
        }
    }
}