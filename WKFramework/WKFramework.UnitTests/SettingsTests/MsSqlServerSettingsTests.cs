using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WKFramework.Settings;
using WKFramework.UnitTests.SettingsTests.TestClasses;
using System.Data.SqlClient;
using WKFramework.Utils;
using System.Collections.Generic;
using WKFramework.UnitTests.Helpers;

namespace WKFramework.UnitTests.SettingsTests
{
    [TestClass]
    public class MsSqlServerSettingsTests
    {
        private const string _connectionString = "Server=(localdb)\\v11.0;Integrated Security=true";
        private const string _dbName = "SettingsDB.Test";
        private const string _tableName = "Settings";

        [TestInitialize]
        public void TestInitialize()
        {
            string filePath = String.Format(@"{0}\{1}.mdf", AppUtils.AssemblyDirectory, _dbName);

            string sql = String.Format(@"IF NOT EXISTS(SELECT name FROM master.dbo.sysdatabases WHERE '[' + name + ']' = N'{0}' OR name = N'{0}')
                                            CREATE DATABASE [{0}] ON PRIMARY (NAME = '{0}', FILENAME = '{2}');

                                         IF EXISTS(SELECT * FROM [{0}].INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{1}')
                                            DROP TABLE [{0}].dbo.{1}", 
                                        _dbName, _tableName, filePath);

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private MsSqlServerSettings<TestKeyEnum> CreateSettingsAndFill()
        {
            var settings = new MsSqlServerSettings<TestKeyEnum>(_connectionString, _tableName, _dbName);
            FillSettings(settings);
            return settings;
        }

        private void FillSettings(MsSqlServerSettings<TestKeyEnum> settings)
        {
            settings.WriteValue(TestKeyEnum.Key1, "value1");
            settings.WriteValue(TestKeyEnum.Key2, TestValueEnum.Value2);
            settings.WriteValue(TestKeyEnum.Key3, TestValueEnum.Value3);
        }

        [TestMethod]
        public void WriteReadStringAsKeyTest()
        {
            var settings = new MsSqlServerSettings<string>(_connectionString, _tableName, _dbName);

            settings.WriteValue("option1", "value1");
            settings.WriteValue("option2", TestValueEnum.Value2);

            Assert.AreEqual("value1", settings.ReadValue<string>("option1"));
            Assert.AreEqual(TestValueEnum.Value2, settings.ReadValue<TestValueEnum>("option2"));
        }

        [TestMethod]
        public void WriteReadEnumAsKeyTest()
        {
            var settings = new MsSqlServerSettings<TestKeyEnum>(_connectionString, _tableName, _dbName);

            settings.WriteValue(TestKeyEnum.Key1, "value1");
            settings.WriteValue(TestKeyEnum.Key2, TestValueEnum.Value2);

            Assert.AreEqual("value1", settings.ReadValue<string>(TestKeyEnum.Key1));
            Assert.AreEqual(TestValueEnum.Value2, settings.ReadValue<TestValueEnum>(TestKeyEnum.Key2));
        }

        [TestMethod]
        public void WriteManyTest()
        {
            var settings = new MsSqlServerSettings<TestKeyEnum>(_connectionString, _tableName, _dbName);

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
        public void ReadManyTest()
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
        public void ReadAllTest()
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
        public void ReadDefaultValueTest()
        {
            var settings = new MsSqlServerSettings<TestKeyEnum>(_connectionString, _tableName, _dbName);
            settings.WriteValue(TestKeyEnum.Key1, "value1");

            Assert.IsNull(settings.ReadValue<string>(TestKeyEnum.Key2));
            Assert.AreEqual("unavailable", settings.ReadValue(TestKeyEnum.Key2, "unavailable"));

            Assert.AreEqual(0, settings.ReadValue<int>(TestKeyEnum.Key2));
            Assert.AreEqual(-1, settings.ReadValue<int>(TestKeyEnum.Key2, -1));
        }

        [TestMethod]
        public void RemoveTest()
        {
            var settings = CreateSettingsAndFill();

            settings.RemoveValue(TestKeyEnum.Key2);
            Assert.AreEqual(2, settings.ReadAll().Count);
            Assert.IsNotNull(settings.ReadValue(TestKeyEnum.Key1));
            Assert.IsNotNull(settings.ReadValue(TestKeyEnum.Key3));

            settings.RemoveValue(TestKeyEnum.Key3);
            Assert.AreEqual(1, settings.ReadAll().Count);
            Assert.IsNotNull(settings.ReadValue(TestKeyEnum.Key1));

            settings.RemoveValue(TestKeyEnum.Key1);
            Assert.AreEqual(0, settings.ReadAll().Count);
        }

        [TestMethod]
        public void RemoveManyTest()
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
        public void RemoveAllTest()
        {
            var settings = CreateSettingsAndFill();

            Assert.AreEqual(3, settings.ReadAll().Count);
            settings.RemoveAll();
            Assert.AreEqual(0, settings.ReadAll().Count);
        }

        [TestMethod]
        public void RemoveResultTest()
        {
            var settings = CreateSettingsAndFill();

            Assert.IsFalse(settings.RemoveValue(TestKeyEnum.Key4));
            Assert.IsTrue(settings.RemoveValue(TestKeyEnum.Key3));

            Assert.IsTrue(settings.RemoveMany(new TestKeyEnum[]{}));
            Assert.IsFalse(settings.RemoveMany(new TestKeyEnum[] { TestKeyEnum.Key3, TestKeyEnum.Key4 }));
            Assert.IsTrue(settings.RemoveMany(new TestKeyEnum[] { TestKeyEnum.Key1, TestKeyEnum.Key2 }));

            FillSettings(settings);
            Assert.IsFalse(settings.RemoveMany(new TestKeyEnum[] { TestKeyEnum.Key3, TestKeyEnum.Key4 }));
        }

        [TestMethod]
        public void CustomKeyConversionTest()
        {
            var settings = new MsSqlServerSettings<TestKeyEnum>(_connectionString, _tableName, _dbName);
            settings.SetKeyConversion(x => "key_" + x.ToString());
            FillSettings(settings);

            var options = settings.ReadAll();
            Assert.IsTrue(options.ContainsKey("key_" + TestKeyEnum.Key1.ToString()));
            Assert.IsTrue(options.ContainsKey("key_" + TestKeyEnum.Key2.ToString()));
            Assert.IsTrue(options.ContainsKey("key_" + TestKeyEnum.Key3.ToString()));

            Assert.AreEqual("value1", settings.ReadValue(TestKeyEnum.Key1));
            Assert.IsTrue(settings.RemoveValue(TestKeyEnum.Key2));

            options = settings.ReadAll();
            Assert.AreEqual(2, options.Count);
            Assert.IsTrue(options.ContainsKey("key_" + TestKeyEnum.Key1.ToString()));
            Assert.IsTrue(options.ContainsKey("key_" + TestKeyEnum.Key3.ToString()));

            settings.RemoveAll();
            settings.SetKeyConversion(null);
            FillSettings(settings);

            options = settings.ReadAll();
            Assert.IsTrue(options.ContainsKey(TestKeyEnum.Key1.ToString()));
            Assert.IsTrue(options.ContainsKey(TestKeyEnum.Key2.ToString()));
            Assert.IsTrue(options.ContainsKey(TestKeyEnum.Key3.ToString()));
        }

        [TestMethod]
        public void CustomValueDBTypeTest()
        {
            var settings = new MsSqlServerSettings<TestKeyEnum>(_connectionString, _tableName, _dbName, System.Data.SqlDbType.NVarChar, "200", new ToStringSerializer());

            settings.WriteValue(TestKeyEnum.Key1, "value1");
            settings.WriteValue(TestKeyEnum.Key2, TestValueEnum.Value2);

            Assert.AreEqual("value1", settings.ReadValue<string>(TestKeyEnum.Key1));
            Assert.AreEqual(TestValueEnum.Value2, settings.ReadValue<TestValueEnum>(TestKeyEnum.Key2));

            Assert.IsTrue(settings.RemoveValue(TestKeyEnum.Key1));
            Assert.AreEqual(1, settings.ReadAll().Count);
        }

        [TestMethod]
        public void LoadPropertiesTest()
        {
            var settings = new MsSqlServerSettings<string>(_connectionString, _tableName, _dbName);

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
        public void SavePropertiesTest()
        {
            var settings = new MsSqlServerSettings<string>(_connectionString, _tableName, _dbName);

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
        public void RemovePropertiesTest()
        {
            var settings = new MsSqlServerSettings<string>(_connectionString, _tableName, _dbName);

            var person = new Person()
            {
                FirstName = "John",
                LastName = "Smith",
                DateOfBirth = new DateTime(1990, 06, 05),
                Height = 175,
            };
            Assert.IsTrue(settings.SaveProperties(person));
            Assert.AreEqual(6, settings.ReadAll().Count);

            settings.WriteValue("TestKey", "test");

            settings.RemoveProperties(person);
            var allSettings = settings.ReadAll();
            Assert.AreEqual(1, allSettings.Count);
            Assert.IsTrue(allSettings.ContainsKey("TestKey"));
        }

        [TestMethod]
        public void NullTest()
        {
            var settings = new MsSqlServerSettings<string>(_connectionString, _tableName, _dbName);

            settings.WriteValue("Key1", null);
            Assert.IsNull(settings.ReadValue<string>("Key1", "default"));

            settings.WriteValue("Key1", "test");
            Assert.AreEqual("test", settings.ReadValue<string>("Key1", "default"));
            settings.WriteValue("Key1", null);
            Assert.IsNull(settings.ReadValue<string>("Key1", "default"));

            AssertExt.ThrowsException<ArgumentNullException>(() => settings.WriteValue(null, "test"));
            AssertExt.ThrowsException<ArgumentNullException>(() => settings.WriteMany(new Dictionary<string, object>() { { "Key1", "test" }, { null, "test" } }));
            AssertExt.ThrowsException<ArgumentNullException>(() => settings.RemoveValue(null));
            AssertExt.ThrowsException<ArgumentNullException>(() => settings.RemoveMany(new string[] { "Key1", null }));
            AssertExt.ThrowsException<ArgumentNullException>(() => settings.LoadProperties(null));
            AssertExt.ThrowsException<ArgumentNullException>(() => settings.SaveProperties(null));
            AssertExt.ThrowsException<ArgumentNullException>(() => settings.RemoveProperties(null));
            
        }

        [TestMethod]
        public void EmptyKeyStringTest()
        {
            var settings = new MsSqlServerSettings<string>(_connectionString, _tableName, _dbName);
            Assert.IsTrue(settings.WriteValue(String.Empty, "test"));
            Assert.AreEqual("test", settings.ReadValue<string>(String.Empty));
            Assert.IsTrue(settings.RemoveValue(String.Empty));
            Assert.IsNull(settings.ReadValue<string>(String.Empty));
        }

        [TestMethod]
        public void SpecialCharactersTest()
        {
            var settings = new MsSqlServerSettings<string>(_connectionString, _tableName, _dbName);

            settings.WriteValue("Key1", "śćżśłółęźć€");
            Assert.AreEqual("śćżśłółęźć€", settings.ReadValue("Key1"));

            settings.WriteValue("Key2", "榜样比如特例");
            Assert.AreEqual("榜样比如特例", settings.ReadValue("Key2"));
        }

        [TestMethod]
        public void ValueUpdateTest()
        {
            var settings = new MsSqlServerSettings<string>(_connectionString, _tableName, _dbName);

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
        public void EmptyDBNameTest()
        {
            new MsSqlServerSettings<string>(_connectionString, _tableName, _dbName); //to create DB and table
            AssertExt.ThrowsException<ArgumentException>(() => new MsSqlServerSettings<string>(_connectionString, _tableName)); //no dbName
            var settings = new MsSqlServerSettings<string>(_connectionString + "; Initial Catalog = " + _dbName, _tableName);
            settings.WriteValue("Key", "Value");
            Assert.AreEqual("Value", settings.ReadValue("Key"));
        }

        [TestMethod]
        public void TooLongKeyTest()
        {
            var settings = new MsSqlServerSettings<string>(_connectionString, _tableName, _dbName);

            var testKey = "".PadRight(MsSqlServerSettings<string>.MaxKeyLength, 'a');
            settings.WriteValue(testKey, "value");
            Assert.AreEqual("value", settings.ReadValue(testKey));

            AssertExt.ThrowsException<ArgumentOutOfRangeException>(() => settings.WriteValue(testKey + 'a', "value"));
        }

        [TestMethod]
        public void ReadWriteManyEmptyKeysTest()
        {
            var settings = new MsSqlServerSettings<string>(_connectionString, _tableName, _dbName);
            Assert.AreEqual(0, settings.ReadMany(new List<string>()).Count);
            Assert.IsTrue(settings.WriteMany(new Dictionary<string, object>()));
        }

        [TestMethod]
        public void RollbackTest()
        {
            var settings = new MsSqlServerSettings<string>(_connectionString, _tableName, _dbName);
            settings.WriteValue("Key", "Value");

            bool shouldThrow = false;
            settings.SetKeyConversion(key =>
                {
                    if (shouldThrow)
                    {
                        throw new InvalidOperationException();
                    }
                    shouldThrow = true;

                    return key.ToString();
                });

            AssertExt.ThrowsException<InvalidOperationException>(() => settings.WriteMany(new Dictionary<string, object>() { { "Key1", "value1" }, { "Key2", "Value2" } }));
            var options = settings.ReadAll();
            Assert.AreEqual(1, options.Count);
            Assert.IsTrue(options.ContainsKey("Key"));
        }

        [TestMethod]
        public void DbValueTypeNoSizeLimitTest()
        {
            var settings = new MsSqlServerSettings<string>(_connectionString, _tableName, _dbName, System.Data.SqlDbType.Int, null, new NoneSerializer());
            settings.WriteValue("Key", 100);
            Assert.AreEqual(100, settings.ReadValue("Key"));
        }
    }
}
