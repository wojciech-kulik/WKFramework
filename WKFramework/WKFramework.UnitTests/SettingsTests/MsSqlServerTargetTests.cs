using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WKFramework.Settings.Targets;
using WKFramework.UnitTests.SettingsTests.TestClasses;

namespace WKFramework.UnitTests.SettingsTests
{
    [TestClass]
    public class MsSqlServerTargetTests
    {
        private const string _connectionString = "Server=(localdb)\\v11.0;Integrated Security=true";
        private const string _dbName = "SettingsDB.Test";

        [TestMethod]
        public void StringAsKeyTest()
        {
            var target = new MsSqlServerTarget<string>(_connectionString, "Settings", _dbName);

            target.WriteValue<string>("option1", "value1");
            target.WriteValue<TestValueEnum>("option2", TestValueEnum.Value2);

            Assert.AreEqual("value1", target.ReadValue<string>("option1"));
            Assert.AreEqual(TestValueEnum.Value2, target.ReadValue<TestValueEnum>("option2"));
        }

        [TestMethod]
        public void EnumAsKeyTest()
        {
            var target = new MsSqlServerTarget<TestKeyEnum>(_connectionString, "Settings2", _dbName);

            target.WriteValue<string>(TestKeyEnum.Key1, "value1");
            target.WriteValue<TestValueEnum>(TestKeyEnum.Key2, TestValueEnum.Value2);

            Assert.AreEqual("value1", target.ReadValue<string>(TestKeyEnum.Key1));
            Assert.AreEqual(TestValueEnum.Value2, target.ReadValue<TestValueEnum>(TestKeyEnum.Key2));
        }
    }
}
