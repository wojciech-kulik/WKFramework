using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WKFramework.Settings;
using UnitTests.SettingsTests.TestClasses;
using WKFramework.Utils.Serializer;

namespace UnitTests.SettingsTests
{
    [TestClass]
    public class FileSettingsTests : CommonSettingsTests
    {
        private string _filePath = "settings.dat";

        protected override ISettings CreateSimpleSettings()
        {
            return new FileSettings(_filePath);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            File.Delete(_filePath);
        }

        [TestMethod]
        public void ShouldBeCopied()
        {
            var settings = CreateSimpleSettings();

            settings.WriteValue(TestKeyEnum.Key1, "value1");
            Assert.AreNotSame(settings.ReadValue(TestKeyEnum.Key1), settings.ReadValue(TestKeyEnum.Key1));

            var person = new Person()
            {
                FirstName = "John",
                LastName = "Smith",
                DateOfBirth = new DateTime(1990, 06, 05),
                Height = 175,
                City = "Los Santos",
                Car = new Car() { Model = "model", Year = new DateTime(2010, 02, 03) }
            };
            settings.WriteValue(TestKeyEnum.Key2, person);

            var person1 = settings.ReadValue<Person>(TestKeyEnum.Key2);
            var person2 = settings.ReadValue<Person>(TestKeyEnum.Key2);
            Assert.AreNotSame(person1, person2);
            Assert.AreNotSame(person1.Car, person2.Car);

            Assert.AreEqual(person1.Car.Model, person2.Car.Model);
            Assert.AreEqual(person1.City, person2.City);
            person1.Car.Model = "changed";
            person2.City = "Las Vegas";
            Assert.AreNotEqual(person1.Car.Model, person2.Car.Model);
            Assert.AreNotEqual(person1.City, person2.City);
        }

        [TestMethod]
        public void KeyConvertionAsNull()
        {
            var settings = CreateSimpleSettings();
            settings.SetKeyConversion(null);
            FillSettings(settings);

            var options = settings.ReadAll();
            Assert.IsTrue(options.ContainsKey(TestKeyEnum.Key1));
            Assert.IsTrue(options.ContainsKey(TestKeyEnum.Key2));
            Assert.IsTrue(options.ContainsKey(TestKeyEnum.Key3));
        }

        [TestMethod]
        public void Load()
        {
            CreateSettingsAndFill();
            var settings = (FileSettings)CreateSimpleSettings();            

            Assert.AreEqual("value1", settings.ReadValue(TestKeyEnum.Key1));
            Assert.AreEqual(TestValueEnum.Value2, settings.ReadValue(TestKeyEnum.Key2));
            Assert.AreEqual(TestValueEnum.Value3, settings.ReadValue(TestKeyEnum.Key3));

            var settings2 = new FileSettings();
            settings2.Load(_filePath);

            Assert.AreEqual("value1", settings2.ReadValue(TestKeyEnum.Key1));
            Assert.AreEqual(TestValueEnum.Value2, settings2.ReadValue(TestKeyEnum.Key2));
            Assert.AreEqual(TestValueEnum.Value3, settings2.ReadValue(TestKeyEnum.Key3));
        }

        [TestMethod]
        public void CustomSerializer()
        {
            var settings = new FileSettings(_filePath, new GZipSerializer());
            FillSettings(settings);

            settings = new FileSettings(_filePath, new GZipSerializer());
            var options = settings.ReadAll();
            Assert.IsTrue(options.ContainsKey(TestKeyEnum.Key1));
            Assert.IsTrue(options.ContainsKey(TestKeyEnum.Key2));
            Assert.IsTrue(options.ContainsKey(TestKeyEnum.Key3));
        }

        [TestMethod]
        public void AutoSaveSwitching()
        {
            var settings = (FileSettings)CreateSimpleSettings();

            settings.WriteValue(TestKeyEnum.Key4, TestValueEnum.Value4);
            settings.AutoSave = false;
            FillSettings(settings);
            settings.Load(_filePath);

            //only one entry should be saved
            var options = settings.ReadAll();
            Assert.AreEqual(1, options.Count);
            Assert.AreEqual(TestValueEnum.Value4, options[TestKeyEnum.Key4]);

            //AutoSave is still off, so it shouldn't be saved
            settings.WriteValue("a", "b");
            settings.Load(_filePath);
            Assert.AreEqual(1, options.Count);

            //turn on AutoSave and verify if was saved to file
            settings.AutoSave = true;
            settings.WriteValue("a", "b");
            settings.Load(_filePath);
            Assert.AreEqual(1, options.Count);
            Assert.AreEqual("b", settings.ReadValue("a"));
        }
    }
}
