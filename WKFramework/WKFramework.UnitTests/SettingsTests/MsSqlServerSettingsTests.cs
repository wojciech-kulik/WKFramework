using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WKFramework.Settings;
using WKFramework.UnitTests.SettingsTests.TestClasses;
using System.Data.SqlClient;
using WKFramework.Utils;
using System.Collections.Generic;

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
    }
}
