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
    }
}
