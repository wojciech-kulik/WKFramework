using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.TestClasses;
using System.Collections.Generic;
using WKFramework.Utils;
using WKFramework.Utils.Serializer;

namespace UnitTests.UtilsTests
{
    [TestClass]
    public class DeepCopyTests
    {
        private Shelf PrepareShelf()
        {
            return new Shelf()
            {
                Capacity = 10,
                Height = 20,
                Owner = new Person() { FullName = "Jack Sparrow" },
                Items = new List<Item>() { new Item() { Name = "Item1" }, new Item() { Name = "Item2" } }
            };
        }

        [TestMethod]
        public void CheckIfReferencesAreDifferent()
        {
            var shelf1 = PrepareShelf();
            var shelf2 = shelf1.DeepClone();

            Assert.AreEqual(shelf1, shelf2);

            Assert.AreNotSame(shelf1, shelf2);
            Assert.AreNotSame(shelf1.Height, shelf2.Height);
            Assert.AreNotSame(shelf1.Capacity, shelf2.Capacity);
            Assert.AreNotSame(shelf1.Owner, shelf2.Owner);
            Assert.AreNotSame(shelf1.Owner.FullName, shelf2.Owner.FullName);
            Assert.AreNotSame(shelf1.Items, shelf2.Items);
            Assert.AreNotSame(shelf1.Items[0], shelf2.Items[0]);
            Assert.AreNotSame(shelf1.Items[0].Name, shelf2.Items[0].Name);
            Assert.AreNotSame(shelf1.Items[1], shelf2.Items[1]);
            Assert.AreNotSame(shelf1.Items[1].Name, shelf2.Items[1].Name);
        }

        [TestMethod]
        public void ThrowsIfNotSerializable()
        {
            AssertExt.ThrowsException<ArgumentException>(() => new NotSerializable().DeepClone());
        }
    }
}
