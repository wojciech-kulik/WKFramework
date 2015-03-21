using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WKFramework.Settings.Targets;
using WKFramework.UnitTests.SettingsTests.TestClasses;
using System.Data.SqlClient;
using WKFramework.Utils;
using System.Collections.Generic;

namespace WKFramework.UnitTests.SettingsTests
{
    [TestClass]
    public class MsSqlServerTargetTests
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

        [TestMethod]
        public void StringAsKeyTest()
        {
            var target = new MsSqlServerTarget<string>(_connectionString, _tableName, _dbName);

            target.WriteValue("option1", "value1");
            target.WriteValue("option2", TestValueEnum.Value2);

            Assert.AreEqual("value1", target.ReadValue<string>("option1"));
            Assert.AreEqual(TestValueEnum.Value2, target.ReadValue<TestValueEnum>("option2"));
        }

        [TestMethod]
        public void EnumAsKeyTest()
        {
            var target = new MsSqlServerTarget<TestKeyEnum>(_connectionString, _tableName, _dbName);

            target.WriteValue(TestKeyEnum.Key1, "value1");
            target.WriteValue(TestKeyEnum.Key2, TestValueEnum.Value2);

            Assert.AreEqual("value1", target.ReadValue<string>(TestKeyEnum.Key1));
            Assert.AreEqual(TestValueEnum.Value2, target.ReadValue<TestValueEnum>(TestKeyEnum.Key2));
        }

        [TestMethod]
        public void CustomValueDBTypeTest()
        {
            var target = new MsSqlServerTarget<TestKeyEnum>(_connectionString, _tableName, _dbName, System.Data.SqlDbType.NVarChar, "200", new ToStringSerializer());

            target.WriteValue(TestKeyEnum.Key1, "value1");
            target.WriteValue(TestKeyEnum.Key2, TestValueEnum.Value2);

            Assert.AreEqual("value1", target.ReadValue<string>(TestKeyEnum.Key1));
            Assert.AreEqual(TestValueEnum.Value2, target.ReadValue<TestValueEnum>(TestKeyEnum.Key2));
        }

        [TestMethod]
        public void ReadManyTest()
        {
            var target = new MsSqlServerTarget<TestKeyEnum>(_connectionString, _tableName, _dbName);

            target.WriteValue(TestKeyEnum.Key1, "value1");
            target.WriteValue(TestKeyEnum.Key2, TestValueEnum.Value2);
            target.WriteValue(TestKeyEnum.Key3, TestValueEnum.Value3);

            var result = target.ReadMany(new TestKeyEnum[] { TestKeyEnum.Key2, TestKeyEnum.Key3 });
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(TestValueEnum.Value2, result[TestKeyEnum.Key2]);
            Assert.AreEqual(TestValueEnum.Value3, result[TestKeyEnum.Key3]);

            result = target.ReadMany(new TestKeyEnum[] { TestKeyEnum.Key1, TestKeyEnum.Key2, TestKeyEnum.Key3 });
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("value1", result[TestKeyEnum.Key1]);
            Assert.AreEqual(TestValueEnum.Value2, result[TestKeyEnum.Key2]);
            Assert.AreEqual(TestValueEnum.Value3, result[TestKeyEnum.Key3]);

            result = target.ReadMany(new TestKeyEnum[] { TestKeyEnum.Key4 });
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void WriteManyTest()
        {
            var target = new MsSqlServerTarget<TestKeyEnum>(_connectionString, _tableName, _dbName);

            target.WriteMany(new Dictionary<TestKeyEnum, object>() 
            { 
                { TestKeyEnum.Key1, "value1" },
                { TestKeyEnum.Key3, TestValueEnum.Value3 }
            });

            var result = target.ReadMany(new TestKeyEnum[] { TestKeyEnum.Key1, TestKeyEnum.Key2, TestKeyEnum.Key3, TestKeyEnum.Key4 });
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("value1", result[TestKeyEnum.Key1]);
            Assert.AreEqual(TestValueEnum.Value3, result[TestKeyEnum.Key3]);
        }

        [TestMethod]
        public void ReadDefaultValueTest()
        {
            var target = new MsSqlServerTarget<TestKeyEnum>(_connectionString, _tableName, _dbName);
            target.WriteValue(TestKeyEnum.Key1, "value1");

            Assert.IsNull(target.ReadValue<string>(TestKeyEnum.Key2));
            Assert.AreEqual("unavailable", target.ReadValue(TestKeyEnum.Key2, "unavailable"));

            Assert.AreEqual(0, target.ReadValue<int>(TestKeyEnum.Key2));
            Assert.AreEqual(-1, target.ReadValue<int>(TestKeyEnum.Key2, -1));
        }
    }
}
