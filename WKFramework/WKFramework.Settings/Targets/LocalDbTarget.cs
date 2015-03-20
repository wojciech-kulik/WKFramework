using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using WKFramework.Utils;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace WKFramework.Settings.Targets
{
    public class LocalDbTarget<K> : ISettingsTarget<K>
    {
        private readonly string _keyColumn;
        private readonly string _valueColumn;
        private readonly string _tableName;
        private string _dbName;
        private string _connectionString;
        private BinarySerializer _serializer = new BinarySerializer();

        public LocalDbTarget(string connectionString, string tableName, string dbName = null, string keyColumn = "optionName", string valueColumn = "value")
        {
            _connectionString = connectionString;
            _tableName = tableName;
            _dbName = dbName;
            _keyColumn = keyColumn;
            _valueColumn = valueColumn;

            InitializeDatabase();
        }

        #region Database initialization

        private void InitializeDatabase()
        {
            if (_dbName == null)
            {
                _dbName = ExtractDBNameFromConnectionStr(_connectionString);
                if (String.IsNullOrEmpty(_dbName))
                    throw new ArgumentException("dbName");
            }

            CreateDatabase();
            SetInitialCatalog(_dbName);
            CreateTable();
        }

        private void CreateDatabase()
        {
            string filePath = String.Format(@"{0}\{1}.mdf", AppUtils.AssemblyDirectory, _dbName);
            string sql = 
                String.Format(
                    "IF NOT (EXISTS(SELECT name FROM master.dbo.sysdatabases WHERE '[' + name + ']' = N'{0}' OR name = N'{0}'))" +
                        "CREATE DATABASE [{0}] ON PRIMARY (NAME = '{0}', FILENAME = '{1}');",
                    _dbName, filePath);

            ExecuteCommand(sql);
        }

        private void CreateTable()
        {
            string sql =
                String.Format(
                    @"IF OBJECT_ID (N'{0}', N'U') IS NULL 
                        CREATE TABLE [{0}] (
                            {1} nvarchar(50) NOT NULL,
                            {2} varbinary(MAX),
                            UNIQUE({1})
                        )",
                   _tableName, _keyColumn, _valueColumn);

            ExecuteCommand(sql);
        }

        private void SetInitialCatalog(string databaseName)
        {
            var connectionStrBuilder = new SqlConnectionStringBuilder(_connectionString);
            connectionStrBuilder.InitialCatalog = databaseName;
            _connectionString = connectionStrBuilder.ToString();
        }

        private string ExtractDBNameFromConnectionStr(string connectionString)
        {
            return new SqlConnectionStringBuilder(_connectionString).InitialCatalog;
        }
        #endregion

        #region Database operations

        private int ExecuteCommand(string sql)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        private void ExecuteQuery(string sql, Action<SqlCommand> commandAction, Action<SqlDataReader> resultAction)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                connection.Open();
                commandAction(command);
                using (var reader = command.ExecuteReader())
                {
                    resultAction(reader);
                }
            }
        }

        private int ExecuteCommandInTransaction(string sql, Action<SqlCommand> action)
        {
            int affectedRows = 0;

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                connection.Open();

                var transaction = connection.BeginTransaction();
                try
                {
                    command.Transaction = transaction;

                    if (action != null)
                        action(command);

                    affectedRows = command.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }

            return affectedRows;
        }

        #endregion

        public V ReadValue<V>(K key, V defaultValue = default(V))
        {
            V result = defaultValue;
            string sql = String.Format("SELECT [{0}] FROM [{1}] WHERE [{2}] = @Key", _valueColumn, _tableName, _keyColumn);

            ExecuteQuery(sql, 
                command =>
                {
                    command.Parameters.AddWithValue("@Key", key.ToString());
                }, 
                sqlReader => 
                {
                    if (sqlReader.Read())
                    {
                        result = _serializer.ConvertFromBinary<V>((byte[])sqlReader[0]);
                    }
                });

            return result;
        }

        public IDictionary<K, V> ReadMany<V>(IEnumerable<K> keys)
        {
            throw new NotImplementedException();
        }

        public bool WriteValue<V>(K key, V value)
        {
            string commandSql = String.Format(@"UPDATE [{0}] SET [{2}]=@Value WHERE [{1}]=@Key
                                                IF @@ROWCOUNT = 0
                                                    INSERT INTO [{0}] ([{1}], [{2}]) VALUES (@Key, @Value)",
                                                _tableName, _keyColumn, _valueColumn);

            int affectedRows = ExecuteCommandInTransaction(commandSql, command =>
                {
                    command.Parameters.AddWithValue("@Key", key.ToString());
                    command.Parameters.AddWithValue("@Value", _serializer.ConvertToBinary(value));
                });

            return affectedRows > 0;
        }

        public bool WriteMany<V>(IDictionary<K, V> values)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
