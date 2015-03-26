using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WKFramework.Utils.Serializer;

namespace UnitTests.UtilsTests
{
    [TestClass]
    public class GZipSerializerTests : CommonBinarySerializerTests
    {
        protected override ISerializer<byte[]> CreateSerializer()
        {
            return new GZipSerializer();
        }
    }
}
