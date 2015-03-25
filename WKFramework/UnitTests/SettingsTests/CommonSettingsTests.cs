using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using WKFramework.Settings;
using UnitTests.SettingsTests.TestClasses;
using WKFramework.Utils.UnitTests.Helpers;

namespace UnitTests.SettingsTests
{
    public abstract class CommonSettingsTests
    {
        protected abstract ISettings CreateSimpleSettings();

        protected ISettings CreateSettingsAndFill()
        {
            var settings = CreateSimpleSettings();
            FillSettings(settings);
            return settings;
        }

        protected void FillSettings(ISettings settings)
        {
            settings.WriteValue(TestKeyEnum.Key1, "value1");
            settings.WriteValue(TestKeyEnum.Key2, TestValueEnum.Value2);
            settings.WriteValue(TestKeyEnum.Key3, TestValueEnum.Value3);
        }

        [TestMethod]
        public void WriteReadStringAsKey()
        {
            var settings = CreateSimpleSettings();

            settings.WriteValue("option1", "value1");
            settings.WriteValue("option2", TestValueEnum.Value2);

            Assert.AreEqual("value1", settings.ReadValue<string>("option1"));
            Assert.AreEqual(TestValueEnum.Value2, settings.ReadValue<TestValueEnum>("option2"));
        }

        [TestMethod]
        public void WriteReadEnumAsKey()
        {
            var settings = CreateSimpleSettings();

            settings.WriteValue(TestKeyEnum.Key1, "value1");
            settings.WriteValue(TestKeyEnum.Key2, TestValueEnum.Value2);

            Assert.AreEqual("value1", settings.ReadValue<string>(TestKeyEnum.Key1));
            Assert.AreEqual(TestValueEnum.Value2, settings.ReadValue<TestValueEnum>(TestKeyEnum.Key2));
        }

        [TestMethod]
        public void WriteMany()
        {
            var settings = CreateSimpleSettings();

            settings.WriteMany(new Dictionary<object, object>() 
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
        public void ReadMany()
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
        public void ReadAll()
        {
            var settings = CreateSimpleSettings();
            settings.SetKeyConversion(x => x.ToString());
            FillSettings(settings);

            var result = settings.ReadAll();
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(result[TestKeyEnum.Key1.ToString()], "value1");
            Assert.AreEqual(result[TestKeyEnum.Key2.ToString()], TestValueEnum.Value2);
            Assert.AreEqual(result[TestKeyEnum.Key3.ToString()], TestValueEnum.Value3);

            settings.RemoveAll();
            Assert.AreEqual(0, settings.ReadAll().Count);
        }

        [TestMethod]
        public void ReadDefaultValue()
        {
            var settings = CreateSimpleSettings();
            settings.WriteValue(TestKeyEnum.Key1, "value1");

            Assert.IsNull(settings.ReadValue<string>(TestKeyEnum.Key2));
            Assert.AreEqual("unavailable", settings.ReadValue(TestKeyEnum.Key2, "unavailable"));

            Assert.AreEqual(0, settings.ReadValue<int>(TestKeyEnum.Key2));
            Assert.AreEqual(-1, settings.ReadValue<int>(TestKeyEnum.Key2, -1));
        }

        [TestMethod]
        public void Remove()
        {
            var settings = CreateSettingsAndFill();

            settings.Remove(TestKeyEnum.Key2);
            Assert.AreEqual(2, settings.ReadAll().Count);
            Assert.IsNotNull(settings.ReadValue(TestKeyEnum.Key1));
            Assert.IsNotNull(settings.ReadValue(TestKeyEnum.Key3));

            settings.Remove(TestKeyEnum.Key3);
            Assert.AreEqual(1, settings.ReadAll().Count);
            Assert.IsNotNull(settings.ReadValue(TestKeyEnum.Key1));

            settings.Remove(TestKeyEnum.Key1);
            Assert.AreEqual(0, settings.ReadAll().Count);
        }

        [TestMethod]
        public void RemoveMany()
        {
            var settings = CreateSettingsAndFill();

            settings.RemoveMany(new TestKeyEnum[] { TestKeyEnum.Key2 });
            Assert.AreEqual(2, settings.ReadAll().Count);
            Assert.IsNotNull(settings.ReadValue(TestKeyEnum.Key1));
            Assert.IsNotNull(settings.ReadValue(TestKeyEnum.Key3));

            settings.RemoveMany(new TestKeyEnum[] { TestKeyEnum.Key1, TestKeyEnum.Key3 });
            Assert.AreEqual(0, settings.ReadAll().Count);
        }

        [TestMethod]
        public void RemoveAll()
        {
            var settings = CreateSettingsAndFill();

            Assert.AreEqual(3, settings.ReadAll().Count);
            settings.RemoveAll();
            Assert.AreEqual(0, settings.ReadAll().Count);
        }

        [TestMethod]
        public void RemoveResult()
        {
            var settings = CreateSettingsAndFill();

            Assert.IsFalse(settings.Remove(TestKeyEnum.Key4));
            Assert.IsTrue(settings.Remove(TestKeyEnum.Key3));

            Assert.IsTrue(settings.RemoveMany(new TestKeyEnum[]{}));
            Assert.IsFalse(settings.RemoveMany(new TestKeyEnum[] { TestKeyEnum.Key3, TestKeyEnum.Key4 }));
            Assert.IsTrue(settings.RemoveMany(new TestKeyEnum[] { TestKeyEnum.Key1, TestKeyEnum.Key2 }));

            FillSettings(settings);
            Assert.IsFalse(settings.RemoveMany(new TestKeyEnum[] { TestKeyEnum.Key3, TestKeyEnum.Key4 }));
        }

        [TestMethod]
        public void CustomKeyConversion()
        {
            var settings = CreateSimpleSettings();
            settings.SetKeyConversion(x => "key_" + x.ToString());
            FillSettings(settings);

            var options = settings.ReadAll();
            Assert.IsTrue(options.ContainsKey("key_" + TestKeyEnum.Key1.ToString()));
            Assert.IsTrue(options.ContainsKey("key_" + TestKeyEnum.Key2.ToString()));
            Assert.IsTrue(options.ContainsKey("key_" + TestKeyEnum.Key3.ToString()));

            Assert.AreEqual("value1", settings.ReadValue(TestKeyEnum.Key1));
            Assert.IsTrue(settings.Remove(TestKeyEnum.Key2));

            options = settings.ReadAll();
            Assert.AreEqual(2, options.Count);
            Assert.IsTrue(options.ContainsKey("key_" + TestKeyEnum.Key1.ToString()));
            Assert.IsTrue(options.ContainsKey("key_" + TestKeyEnum.Key3.ToString()));
        }

        [TestMethod]
        public void LoadProperties()
        {
            var settings = CreateSimpleSettings();

            settings.WriteValue("Person.FirstName", "John");
            settings.WriteValue("Person.LastName", "Smith");
            settings.WriteValue("Person.DateOfBirth", new DateTime(1990, 06, 05));
            settings.WriteValue("Person.Height", 175);
            settings.WriteValue("BasePerson.City", "Los Angeles");

            var person = new Person();
            settings.LoadProperties(person);

            Assert.AreEqual("John", person.FirstName);
            Assert.AreEqual("Smith", person.LastName);
            Assert.AreEqual(new DateTime(1990, 06, 05), person.DateOfBirth);
            Assert.AreEqual(175, person.Height);
            Assert.AreEqual("Los Angeles", person.City);
            Assert.IsNull(person.PhoneNumber);
        }

        [TestMethod]
        public void SaveProperties()
        {
            var settings = CreateSimpleSettings();

            var person = new Person()
            {
                FirstName = "John",
                LastName = "Smith",
                DateOfBirth = new DateTime(1990, 06, 05),
                Height = 175,
                City = "Los Angeles"
            };
            person.SetAddress("Shouldn't be saved");
            Assert.IsTrue(settings.SaveProperties(person));

            var loadedPerson = new Person();
            settings.LoadProperties(loadedPerson);

            Assert.AreEqual(person.FirstName, loadedPerson.FirstName);
            Assert.AreEqual(person.LastName, loadedPerson.LastName);
            Assert.AreEqual(person.DateOfBirth, loadedPerson.DateOfBirth);
            Assert.AreEqual(person.Height, loadedPerson.Height);
            Assert.AreEqual(person.PhoneNumber, loadedPerson.PhoneNumber);
            Assert.AreEqual(person.City, loadedPerson.City);
            Assert.IsNull(settings.ReadValue("Person.Address"));
            Assert.IsNotNull(settings.ReadValue("BasePerson.City"));
        }

        [TestMethod]
        public void RemoveProperties()
        {
            var settings = CreateSimpleSettings();

            var person = new Person()
            {
                FirstName = "John",
                LastName = "Smith",
                DateOfBirth = new DateTime(1990, 06, 05),
                Height = 175,
            };
            Assert.IsTrue(settings.SaveProperties(person));
            Assert.AreEqual(7, settings.ReadAll().Count);

            settings.WriteValue("TestKey", "test");

            settings.RemoveProperties(person);
            var allSettings = settings.ReadAll();
            Assert.AreEqual(1, allSettings.Count);
            Assert.IsTrue(allSettings.ContainsKey("TestKey"));
        }

        [TestMethod]
        public void NullAsArgument()
        {
            var settings = CreateSimpleSettings();

            settings.WriteValue("Key1", null);
            Assert.IsNull(settings.ReadValue<string>("Key1", "default"));

            settings.WriteValue("Key1", "test");
            Assert.AreEqual("test", settings.ReadValue<string>("Key1", "default"));
            settings.WriteValue("Key1", null);
            Assert.IsNull(settings.ReadValue<string>("Key1", "default"));

            AssertExt.ThrowsException<ArgumentNullException>(() => settings.WriteValue(null, "test"));
            AssertExt.ThrowsException<ArgumentNullException>(() => settings.WriteMany(new Dictionary<object, object>() { { "Key1", "test" }, { null, "test" } }));
            AssertExt.ThrowsException<ArgumentNullException>(() => settings.Remove(null));
            AssertExt.ThrowsException<ArgumentNullException>(() => settings.RemoveMany(new string[] { "Key1", null }));
            AssertExt.ThrowsException<ArgumentNullException>(() => settings.LoadProperties(null));
            AssertExt.ThrowsException<ArgumentNullException>(() => settings.SaveProperties(null));
            AssertExt.ThrowsException<ArgumentNullException>(() => settings.RemoveProperties(null));
            
        }

        [TestMethod]
        public void EmptyKeyString()
        {
            var settings = CreateSimpleSettings();
            Assert.IsTrue(settings.WriteValue(String.Empty, "test"));
            Assert.AreEqual("test", settings.ReadValue<string>(String.Empty));
            Assert.IsTrue(settings.Remove(String.Empty));
            Assert.IsNull(settings.ReadValue<string>(String.Empty));
        }

        [TestMethod]
        public void SpecialCharacters()
        {
            var settings = CreateSimpleSettings();

            settings.WriteValue("Key1", "śćżśłółęźć€");
            Assert.AreEqual("śćżśłółęźć€", settings.ReadValue("Key1"));

            settings.WriteValue("Key2", "榜样比如特例");
            Assert.AreEqual("榜样比如特例", settings.ReadValue("Key2"));
        }

        [TestMethod]
        public void ValueUpdate()
        {
            var settings = CreateSimpleSettings();

            settings.WriteValue("Key1", "value1");
            Assert.AreEqual("value1", settings.ReadValue<string>("Key1"));

            settings.WriteValue("Key1", "value2");
            Assert.AreEqual("value2", settings.ReadValue<string>("Key1"));
            Assert.AreEqual(1, settings.ReadAll().Count);

            settings.WriteValue("Key2", "value3");
            Assert.AreEqual("value2", settings.ReadValue<string>("Key1"));
            Assert.AreEqual("value3", settings.ReadValue<string>("Key2"));
            Assert.AreEqual(2, settings.ReadAll().Count);
        }

        [TestMethod]
        public void ReadWriteManyWithEmptyKeys()
        {
            var settings = CreateSimpleSettings();
            Assert.AreEqual(0, settings.ReadMany(new List<string>()).Count);
            Assert.IsTrue(settings.WriteMany(new Dictionary<object, object>()));
        }

        [TestMethod]
        public void ClassValue()
        {
            var settings = CreateSimpleSettings();

            var person = new Person()
            {
                FirstName = "John",
                LastName = "Smith",
                DateOfBirth = new DateTime(1990, 06, 05),
                Height = 175,
                City = "Los Santos",
                Car = new Car() {  Model = "model", Year = new DateTime(2010, 02, 03) }
            };

            Assert.IsTrue(settings.WriteValue("person", person));

            var loadedPerson = settings.ReadValue<Person>("person");
            Assert.IsNotNull(loadedPerson);

            Assert.AreEqual(person.FirstName, loadedPerson.FirstName);
            Assert.AreEqual(person.LastName, loadedPerson.LastName);
            Assert.AreEqual(person.DateOfBirth, loadedPerson.DateOfBirth);
            Assert.AreEqual(person.Height, loadedPerson.Height);
            Assert.AreEqual(person.PhoneNumber, loadedPerson.PhoneNumber);
            Assert.AreEqual(person.City, loadedPerson.City);
            Assert.AreEqual(person.Car.Model, loadedPerson.Car.Model);
            Assert.AreEqual(person.Car.Year, loadedPerson.Car.Year);
        }
    }
}
