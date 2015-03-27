using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTests
{
    public static class AssertExt
    {
        public static void ThrowsException<T>(Action action)
            where T : Exception
        {
            try
            {
                action();
                Assert.Fail("Exception hasn't been thrown.");
            }
            catch (T)
            {
            }
            catch (Exception ex)
            {
                Assert.Fail("Throws different exception: {0} - {1}", ex.GetType().Name, ex.Message);
            }
        }
    }
}
