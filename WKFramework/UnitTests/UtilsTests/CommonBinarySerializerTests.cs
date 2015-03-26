using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WKFramework.Utils.Serializer;
using UnitTests.TestClasses;
using System.Collections.Generic;

namespace UnitTests.UtilsTests
{
    public abstract class CommonBinarySerializerTests
    {
        private Shelf PrepareShelfToSerialize()
        {
            return new Shelf()
            {
                Capacity = 10,
                Height = 20,
                Owner = new Person() { FullName = "Jack Sparrow" },
                Items = new List<Item>() { new Item() { Name = "Item1" }, new Item() { Name = "Item2" } }
            };
        }

        protected abstract ISerializer<byte[]> CreateSerializer();

        [TestMethod]
        public void SerializeDeserialize()
        {
            var serializer = CreateSerializer();
            var obj = PrepareShelfToSerialize();
            var bytes = serializer.Serialize(obj);

            Assert.IsNotNull(bytes);
            Assert.IsTrue(bytes.Length > 15);

            var deserialized = serializer.Deserialize<Shelf>(bytes);
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(obj, deserialized);
        }

        [TestMethod]
        public void NullArgument()
        {
            var serializer = CreateSerializer();
            Assert.IsNull(serializer.Serialize(null));
            Assert.IsNull(serializer.Deserialize<object>(null));
        }

        [TestMethod]
        public void ISerializerAndGenericISerializer()
        {
            var serializer = CreateSerializer();
            var obj = PrepareShelfToSerialize();
            var bytesGeneric = serializer.Serialize(obj);
            var bytesNonGeneric = (byte[])(serializer as ISerializer).Serialize(obj);
            var bytesSerializer = (serializer as Serializer<byte[]>).Serialize(obj);

            Assert.IsNotNull(bytesGeneric);
            Assert.AreEqual(bytesGeneric.Length, bytesNonGeneric.Length);
            Assert.AreEqual(bytesNonGeneric.Length, bytesSerializer.Length);

            for (int i = 0; i < bytesGeneric.Length; i++)
            {
                Assert.AreEqual(bytesGeneric[i], bytesNonGeneric[i]);
                Assert.AreEqual(bytesNonGeneric[i], bytesSerializer[i]);
            }
        }
    }
}
