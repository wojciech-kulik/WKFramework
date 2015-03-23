using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WKFramework.Settings;
using WKFramework.UnitTests.SettingsTests.TestClasses;

namespace WKFramework.UnitTests.SettingsTests
{
    [TestClass]
    public class FileSettingsTests
    {
        private FileSettings<TestKeyEnum> CreateSettingsAndFill()
        {
            var settings = new FileSettings<TestKeyEnum>("settings.dat");
            FillSettings(settings);
            return settings;
        }

        private void FillSettings(FileSettings<TestKeyEnum> settings)
        {
            settings.WriteValue(TestKeyEnum.Key1, "value1");
            settings.WriteValue(TestKeyEnum.Key2, TestValueEnum.Value2);
            settings.WriteValue(TestKeyEnum.Key3, TestValueEnum.Value3);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            File.Delete("settings.dat");
        }

        [TestMethod]
        public void FileSettings_WriteReadStringAsKey()
        {
            var settings = new FileSettings<string>("settings.dat");

            settings.WriteValue("option1", "value1");
            settings.WriteValue("option2", TestValueEnum.Value2);

            Assert.AreEqual("value1", settings.ReadValue<string>("option1"));
            Assert.AreEqual(TestValueEnum.Value2, settings.ReadValue<TestValueEnum>("option2"));
        }

        [TestMethod]
        public void FileSettings_WriteReadEnumAsKey()
        {
            var settings = new FileSettings<TestKeyEnum>("settings.dat");

            settings.WriteValue(TestKeyEnum.Key1, "value1");
            settings.WriteValue(TestKeyEnum.Key2, TestValueEnum.Value2);

            Assert.AreEqual("value1", settings.ReadValue<string>(TestKeyEnum.Key1));
            Assert.AreEqual(TestValueEnum.Value2, settings.ReadValue<TestValueEnum>(TestKeyEnum.Key2));
        }

        [TestMethod]
        public void FileSettings_WriteMany()
        {
            var settings = new FileSettings<TestKeyEnum>("settings.dat");

            settings.WriteMany(new Dictionary<TestKeyEnum, object>() 
            { 
                { TestKeyEnum.Key1, "value1" },
                { TestKeyEnum.Key3, TestValueEnum.Value3 }
            });

            var result = settings.ReadMany(new TestKeyEnum[] { TestKeyEnum.Key1, TestKeyEnum.Key2, TestKeyEnum.Key3, TestKeyEnum.Key4 });
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("value1", result[TestKeyEnum.Key1]);
            Assert.AreEqual(TestValueEnum.Value3, result[TestKeyEnum.Key3]);
        }

        [TestMethod]
        public void FileSettings_ReadMany()
        {
            var settings = CreateSettingsAndFill();

            var result = settings.ReadMany(new TestKeyEnum[] { TestKeyEnum.Key2, TestKeyEnum.Key3 });
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(TestValueEnum.Value2, result[TestKeyEnum.Key2]);
            Assert.AreEqual(TestValueEnum.Value3, result[TestKeyEnum.Key3]);

            result = settings.ReadMany(new TestKeyEnum[] { TestKeyEnum.Key1, TestKeyEnum.Key2, TestKeyEnum.Key3 });
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("value1", result[TestKeyEnum.Key1]);
            Assert.AreEqual(TestValueEnum.Value2, result[TestKeyEnum.Key2]);
            Assert.AreEqual(TestValueEnum.Value3, result[TestKeyEnum.Key3]);

            var resultWithType = settings.ReadMany<TestValueEnum>(new TestKeyEnum[] { TestKeyEnum.Key4 });
            Assert.AreEqual(0, resultWithType.Count);

            resultWithType = settings.ReadMany<TestValueEnum>(new TestKeyEnum[] { TestKeyEnum.Key3, TestKeyEnum.Key4 });
            Assert.AreEqual(1, resultWithType.Count);
            Assert.AreEqual(TestValueEnum.Value3, resultWithType[TestKeyEnum.Key3]);
        }

        [TestMethod]
        public void FileSettings_ReadAll()
        {
            var settings = CreateSettingsAndFill();

            var result = settings.ReadAll();
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(result[TestKeyEnum.Key1.ToString()], "value1");
            Assert.AreEqual(result[TestKeyEnum.Key2.ToString()], TestValueEnum.Value2);
            Assert.AreEqual(result[TestKeyEnum.Key3.ToString()], TestValueEnum.Value3);

            settings.RemoveAll();
            Assert.AreEqual(0, settings.ReadAll().Count);
        }

        [TestMethod]
        public void FileSettings_ReadDefaultValue()
        {
            var settings = new FileSettings<TestKeyEnum>("settings.dat");
            settings.WriteValue(TestKeyEnum.Key1, "value1");

            Assert.IsNull(settings.ReadValue<string>(TestKeyEnum.Key2));
            Assert.AreEqual("unavailable", settings.ReadValue(TestKeyEnum.Key2, "unavailable"));

            Assert.AreEqual(0, settings.ReadValue<int>(TestKeyEnum.Key2));
            Assert.AreEqual(-1, settings.ReadValue<int>(TestKeyEnum.Key2, -1));
        }
    }
}
