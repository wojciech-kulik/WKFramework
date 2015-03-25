using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WKFramework.Settings;
using UnitTests.SettingsTests.TestClasses;
using System.Data.SqlClient;
using WKFramework.Utils;
using System.Collections.Generic;
using WKFramework.Utils.UnitTests.Helpers;

namespace UnitTests.SettingsTests
{
    [TestClass]
    public class MsSqlServerSettingsTests : CommonSettingsTests
    {
        private const string _connectionString = "Server=(localdb)\\v11.0;Integrated Security=true";
        private const string _dbName = "SettingsDB.Test";
        private const string _tableName = "Settings";

        protected override ISettings CreateSimpleSettings()
        {
            return new MsSqlServerSettings(_connectionString, _tableName, _dbName);
        }

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

        [TestMethod]
        public void CustomValueDBType()
        {
            var settings = new MsSqlServerSettings(_connectionString, _tableName, _dbName, System.Data.SqlDbType.NVarChar, "200", new ToStringSerializer());

            settings.WriteValue(TestKeyEnum.Key1, "value1");
            settings.WriteValue(TestKeyEnum.Key2, TestValueEnum.Value2);

            Assert.AreEqual("value1", settings.ReadValue<string>(TestKeyEnum.Key1));
            Assert.AreEqual(TestValueEnum.Value2, settings.ReadValue<TestValueEnum>(TestKeyEnum.Key2));

            Assert.IsTrue(settings.Remove(TestKeyEnum.Key1));
            Assert.AreEqual(1, settings.ReadAll().Count);
        }

        [TestMethod]
        public void EmptyDBName()
        {
            CreateSimpleSettings(); //to create DB and table
            AssertExt.ThrowsException<ArgumentException>(() => new MsSqlServerSettings(_connectionString, _tableName)); //no dbName
            var settings = new MsSqlServerSettings(_connectionString + "; Initial Catalog = " + _dbName, _tableName);
            settings.WriteValue("Key", "Value");
            Assert.AreEqual("Value", settings.ReadValue("Key"));
        }

        [TestMethod]
        public void TooLongKey()
        {
            var settings = CreateSimpleSettings();
            var testKey = "".PadRight(MsSqlServerSettings.MaxKeyLength, 'a');
            settings.WriteValue(testKey, "value");
            Assert.AreEqual("value", settings.ReadValue(testKey));

            AssertExt.ThrowsException<ArgumentOutOfRangeException>(() => settings.WriteValue(testKey + 'a', "value"));
        }

        [TestMethod]
        public void Rollback()
        {
            var settings = (MsSqlServerSettings)CreateSimpleSettings();
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

            AssertExt.ThrowsException<InvalidOperationException>(() => settings.WriteMany(new Dictionary<object, object>() { { "Key1", "value1" }, { "Key2", "Value2" } }));
            var options = settings.ReadAll();
            Assert.AreEqual(1, options.Count);
            Assert.IsTrue(options.ContainsKey("Key"));
        }

        [TestMethod]
        public void DbValueTypeNoSizeLimit()
        {
            var settings = new MsSqlServerSettings(_connectionString, _tableName, _dbName, System.Data.SqlDbType.Int, null, new NoneSerializer());
            settings.WriteValue("Key", 100);
            Assert.AreEqual(100, settings.ReadValue("Key"));
        }

        [TestMethod]
        public void KeyConvertionAsNull()
        {
            var settings = CreateSimpleSettings();
            settings.SetKeyConversion(null);
            FillSettings(settings);

            var options = settings.ReadAll();
            Assert.IsTrue(options.ContainsKey(TestKeyEnum.Key1.ToString()));
            Assert.IsTrue(options.ContainsKey(TestKeyEnum.Key2.ToString()));
            Assert.IsTrue(options.ContainsKey(TestKeyEnum.Key3.ToString()));
        }

        [TestMethod]
        public void IncorrectKeyConvertion()
        {
            var settings = CreateSimpleSettings();
            settings.SetKeyConversion(x => x);
            AssertExt.ThrowsException<InvalidOperationException>(() => settings.WriteValue(TestKeyEnum.Key1, "value"));
        }
    }
}
