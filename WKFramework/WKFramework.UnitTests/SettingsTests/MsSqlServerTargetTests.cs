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

        private MsSqlServerTarget<TestKeyEnum> CreateTargetAndFill()
        {
            var target = new MsSqlServerTarget<TestKeyEnum>(_connectionString, _tableName, _dbName);
            FillTarget(target);
            return target;
        }

        private void FillTarget(MsSqlServerTarget<TestKeyEnum> target)
        {
            target.WriteValue(TestKeyEnum.Key1, "value1");
            target.WriteValue(TestKeyEnum.Key2, TestValueEnum.Value2);
            target.WriteValue(TestKeyEnum.Key3, TestValueEnum.Value3);
        }

        [TestMethod]
        public void WriteReadStringAsKeyTest()
        {
            var target = new MsSqlServerTarget<string>(_connectionString, _tableName, _dbName);

            target.WriteValue("option1", "value1");
            target.WriteValue("option2", TestValueEnum.Value2);

            Assert.AreEqual("value1", target.ReadValue<string>("option1"));
            Assert.AreEqual(TestValueEnum.Value2, target.ReadValue<TestValueEnum>("option2"));
        }

        [TestMethod]
        public void WriteReadEnumAsKeyTest()
        {
            var target = new MsSqlServerTarget<TestKeyEnum>(_connectionString, _tableName, _dbName);

            target.WriteValue(TestKeyEnum.Key1, "value1");
            target.WriteValue(TestKeyEnum.Key2, TestValueEnum.Value2);

            Assert.AreEqual("value1", target.ReadValue<string>(TestKeyEnum.Key1));
            Assert.AreEqual(TestValueEnum.Value2, target.ReadValue<TestValueEnum>(TestKeyEnum.Key2));
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
        public void ReadManyTest()
        {
            var target = CreateTargetAndFill();

            var result = target.ReadMany(new TestKeyEnum[] { TestKeyEnum.Key2, TestKeyEnum.Key3 });
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(TestValueEnum.Value2, result[TestKeyEnum.Key2]);
            Assert.AreEqual(TestValueEnum.Value3, result[TestKeyEnum.Key3]);

            result = target.ReadMany(new TestKeyEnum[] { TestKeyEnum.Key1, TestKeyEnum.Key2, TestKeyEnum.Key3 });
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("value1", result[TestKeyEnum.Key1]);
            Assert.AreEqual(TestValueEnum.Value2, result[TestKeyEnum.Key2]);
            Assert.AreEqual(TestValueEnum.Value3, result[TestKeyEnum.Key3]);

            var resultWithType = target.ReadMany<TestValueEnum>(new TestKeyEnum[] { TestKeyEnum.Key4 });
            Assert.AreEqual(0, resultWithType.Count);

            resultWithType = target.ReadMany<TestValueEnum>(new TestKeyEnum[] { TestKeyEnum.Key3, TestKeyEnum.Key4 });
            Assert.AreEqual(1, resultWithType.Count);
            Assert.AreEqual(TestValueEnum.Value3, resultWithType[TestKeyEnum.Key3]);
        }

        [TestMethod]
        public void ReadAllTest()
        {
            var target = CreateTargetAndFill();

            var result = target.ReadAll();
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(result[TestKeyEnum.Key1.ToString()], "value1");
            Assert.AreEqual(result[TestKeyEnum.Key2.ToString()], TestValueEnum.Value2);
            Assert.AreEqual(result[TestKeyEnum.Key3.ToString()], TestValueEnum.Value3);

            target.RemoveAll();
            Assert.AreEqual(0, target.ReadAll().Count);
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

        [TestMethod]
        public void RemoveTest()
        {
            var target = CreateTargetAndFill();

            target.RemoveValue(TestKeyEnum.Key2);
            Assert.AreEqual(2, target.ReadAll().Count);
            Assert.IsNotNull(target.ReadValue(TestKeyEnum.Key1));
            Assert.IsNotNull(target.ReadValue(TestKeyEnum.Key3));

            target.RemoveValue(TestKeyEnum.Key3);
            Assert.AreEqual(1, target.ReadAll().Count);
            Assert.IsNotNull(target.ReadValue(TestKeyEnum.Key1));

            target.RemoveValue(TestKeyEnum.Key1);
            Assert.AreEqual(0, target.ReadAll().Count);
        }

        [TestMethod]
        public void RemoveManyTest()
        {
            var target = CreateTargetAndFill();

            target.RemoveMany(new TestKeyEnum[] { TestKeyEnum.Key2 });
            Assert.AreEqual(2, target.ReadAll().Count);
            Assert.IsNotNull(target.ReadValue(TestKeyEnum.Key1));
            Assert.IsNotNull(target.ReadValue(TestKeyEnum.Key3));

            target.RemoveMany(new TestKeyEnum[] { TestKeyEnum.Key1, TestKeyEnum.Key3 });
            Assert.AreEqual(0, target.ReadAll().Count);
        }

        [TestMethod]
        public void RemoveAllTest()
        {
            var target = CreateTargetAndFill();

            Assert.AreEqual(3, target.ReadAll().Count);
            target.RemoveAll();
            Assert.AreEqual(0, target.ReadAll().Count);
        }

        [TestMethod]
        public void RemoveResultTest()
        {
            var target = CreateTargetAndFill();

            Assert.IsFalse(target.RemoveValue(TestKeyEnum.Key4));
            Assert.IsTrue(target.RemoveValue(TestKeyEnum.Key3));

            Assert.IsTrue(target.RemoveMany(new TestKeyEnum[]{}));
            Assert.IsFalse(target.RemoveMany(new TestKeyEnum[] { TestKeyEnum.Key3, TestKeyEnum.Key4 }));
            Assert.IsTrue(target.RemoveMany(new TestKeyEnum[] { TestKeyEnum.Key1, TestKeyEnum.Key2 }));

            FillTarget(target);
            Assert.IsFalse(target.RemoveMany(new TestKeyEnum[] { TestKeyEnum.Key3, TestKeyEnum.Key4 }));
        }

        [TestMethod]
        public void CustomKeyConversionTest()
        {
            var target = new MsSqlServerTarget<TestKeyEnum>(_connectionString, _tableName, _dbName);
            target.SetKeyConversion(x => "key_" + x.ToString());
            FillTarget(target);

            var settings = target.ReadAll();
            Assert.IsTrue(settings.ContainsKey("key_" + TestKeyEnum.Key1.ToString()));
            Assert.IsTrue(settings.ContainsKey("key_" + TestKeyEnum.Key2.ToString()));
            Assert.IsTrue(settings.ContainsKey("key_" + TestKeyEnum.Key3.ToString()));

            Assert.AreEqual("value1", target.ReadValue(TestKeyEnum.Key1));
            Assert.IsTrue(target.RemoveValue(TestKeyEnum.Key2));

            settings = target.ReadAll();
            Assert.AreEqual(2, settings.Count);
            Assert.IsTrue(settings.ContainsKey("key_" + TestKeyEnum.Key1.ToString()));
            Assert.IsTrue(settings.ContainsKey("key_" + TestKeyEnum.Key3.ToString()));

            target.RemoveAll();
            target.SetKeyConversion(null);
            FillTarget(target);

            settings = target.ReadAll();
            Assert.IsTrue(settings.ContainsKey(TestKeyEnum.Key1.ToString()));
            Assert.IsTrue(settings.ContainsKey(TestKeyEnum.Key2.ToString()));
            Assert.IsTrue(settings.ContainsKey(TestKeyEnum.Key3.ToString()));
        }

        [TestMethod]
        public void CustomValueDBTypeTest()
        {
            var target = new MsSqlServerTarget<TestKeyEnum>(_connectionString, _tableName, _dbName, System.Data.SqlDbType.NVarChar, "200", new ToStringSerializer());

            target.WriteValue(TestKeyEnum.Key1, "value1");
            target.WriteValue(TestKeyEnum.Key2, TestValueEnum.Value2);

            Assert.AreEqual("value1", target.ReadValue<string>(TestKeyEnum.Key1));
            Assert.AreEqual(TestValueEnum.Value2, target.ReadValue<TestValueEnum>(TestKeyEnum.Key2));

            Assert.IsTrue(target.RemoveValue(TestKeyEnum.Key1));
            Assert.AreEqual(1, target.ReadAll().Count);
        }
    }
}
