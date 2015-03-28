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
            var settings = new FileSettings();
            settings.Load(_filePath);

            var options = settings.ReadAll();
            Assert.AreEqual(3, options.Count);
            Assert.AreEqual("value1", options[TestKeyEnum.Key1]);
            Assert.AreEqual(TestValueEnum.Value2, options[TestKeyEnum.Key2]);
            Assert.AreEqual(TestValueEnum.Value3, options[TestKeyEnum.Key3]);
        }

        private void TestIfSettingsContainsOptionsAfterReadFromFile(Action<FileSettings> action)
        {
            var settings = (FileSettings)CreateSimpleSettings();
            action(settings);

            settings.Load(_filePath);
            var options = settings.ReadAll();
            Assert.AreEqual(2, options.Count);
            Assert.AreEqual("1", options[TestKeyEnum.Key1]);
            Assert.AreEqual("2", options[TestKeyEnum.Key2]);

            settings = (FileSettings)CreateSimpleSettings();
            options = settings.ReadAll();
            Assert.AreEqual(2, options.Count);
            Assert.AreEqual("1", options[TestKeyEnum.Key1]);
            Assert.AreEqual("2", options[TestKeyEnum.Key2]);
        }

        [TestMethod]
        public void ReadFromFileAfterWrite()
        {
            TestIfSettingsContainsOptionsAfterReadFromFile(settings =>
                {
                    settings.WriteValue(TestKeyEnum.Key1, "1");
                    settings.WriteValue(TestKeyEnum.Key2, "2");
                });
        }

        [TestMethod]
        public void ReadFromFileAfterWriteMany()
        {
            TestIfSettingsContainsOptionsAfterReadFromFile(settings => settings.WriteMany(new Dictionary<object, object>() { { TestKeyEnum.Key1, "1" }, { TestKeyEnum.Key2, "2" } }));
        }

        [TestMethod]
        public void ReadFromFileAfterWriteProperties()
        {
            var settings = CreateSimpleSettings();

            Person.StaticProperty = "test";
            var person = new Person()
            {
                FirstName = "John",
                LastName = "Smith",
                DateOfBirth = new DateTime(1990, 06, 05),
                Height = 175,
                City = "Los Angeles"
            };
            person.SetAddress("Shouldn't be saved");
            Assert.IsTrue(settings.WriteProperties(person));
            Person.StaticProperty = null;

            var loadedPerson = new Person();
            settings = CreateSimpleSettings();
            settings.ReadProperties(loadedPerson);

            Assert.AreEqual(person.FirstName, loadedPerson.FirstName);
            Assert.AreEqual(person.LastName, loadedPerson.LastName);
            Assert.AreEqual(person.DateOfBirth, loadedPerson.DateOfBirth);
            Assert.AreEqual(person.Height, loadedPerson.Height);
            Assert.AreEqual(person.PhoneNumber, loadedPerson.PhoneNumber);
            Assert.AreEqual(person.City, loadedPerson.City);
            Assert.AreEqual("test", Person.StaticProperty);
            Assert.IsNull(settings.ReadValue("Person.Address"));
            Assert.IsNotNull(settings.ReadValue("BasePerson.City"));
        }

        [TestMethod]
        public void ReadFromFileAfterRemoveProperties()
        {
            var settings = CreateSimpleSettings();

            var person = new Person()
            {
                FirstName = "John",
                LastName = "Smith",
                DateOfBirth = new DateTime(1990, 06, 05),
                Height = 175,
            };
            Assert.IsTrue(settings.WriteProperties(person));

            settings = CreateSimpleSettings();
            Assert.AreEqual(9, settings.ReadAll().Count);
            settings.WriteValue("TestKey", "test");
            settings.RemoveProperties(person);

            settings = CreateSimpleSettings();
            var allSettings = settings.ReadAll();
            Assert.AreEqual(1, allSettings.Count);
            Assert.IsTrue(allSettings.ContainsKey("TestKey"));
        }

        [TestMethod]
        public void ReadFromFileAfterRemove()
        {
            var settings = (FileSettings)CreateSettingsAndFill();           
            settings.Load(_filePath);
            settings.Remove(TestKeyEnum.Key1);

            settings = (FileSettings)CreateSimpleSettings();
            var options = settings.ReadAll();
            Assert.AreEqual(2, options.Count);
            Assert.AreEqual(TestValueEnum.Value2, options[TestKeyEnum.Key2]);
            Assert.AreEqual(TestValueEnum.Value3, options[TestKeyEnum.Key3]);
        }

        [TestMethod]
        public void ReadFromFileAfterRemoveMany()
        {
            var settings = (FileSettings)CreateSettingsAndFill();
            settings.Load(_filePath);
            settings.RemoveMany(new object[] { TestKeyEnum.Key1, TestKeyEnum.Key2 });

            settings = (FileSettings)CreateSimpleSettings();
            var options = settings.ReadAll();
            Assert.AreEqual(1, options.Count);
            Assert.AreEqual(TestValueEnum.Value3, options[TestKeyEnum.Key3]);
        }

        [TestMethod]
        public void ReadFromFileAfterRemoveAll()
        {
            var settings = (FileSettings)CreateSettingsAndFill();
            settings.Load(_filePath);
            settings.RemoveAll();

            settings = (FileSettings)CreateSimpleSettings();
            Assert.AreEqual(0, settings.ReadAll().Count);
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
