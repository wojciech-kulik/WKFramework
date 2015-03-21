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
using WKFramework.Utils.Serializer;
using System.Data;

namespace WKFramework.Settings.Targets
{
    public class MsSqlServerTarget<TKey> : ISettingsTarget<TKey>
    {
        private readonly string _keyColumn = "optionName";
        private readonly string _valueColumn = "value";
        private readonly string _tableName;
        private readonly SqlDbType _valueDbType = SqlDbType.VarBinary;
        private readonly string _valueDbTypeSizeLimit = "MAX";
        private string _dbName;
        private string _connectionString;
        private ISerializer _serializer = new BinarySerializer();

        public MsSqlServerTarget(string connectionString, string tableName, string dbName = null)
        {
            _connectionString = connectionString;
            _tableName = tableName;
            _dbName = dbName;

            InitializeDatabase();
        }

        public MsSqlServerTarget(string connectionString, string tableName, string dbName,
                                 SqlDbType valueDbType, string valueDbTypeSizeLimit,
                                 ISerializer serializer)
        {
            _connectionString = connectionString;
            _tableName = tableName;
            _dbName = dbName;
            _valueDbType = valueDbType;
            _valueDbTypeSizeLimit = valueDbTypeSizeLimit;
            _serializer = serializer;

            InitializeDatabase();
        }

        #region SQL Queries

        private const string KeyParam = "@Key";

        private const string ValueParam = "@Value";

        private const string SelectQuery = "SELECT [{0}] FROM [{1}] WHERE [{2}] = @Key";

        private const string UpdateQuery = @"UPDATE [{0}] SET [{2}]=@Value WHERE [{1}]=@Key
                                             IF @@ROWCOUNT = 0
                                                INSERT INTO [{0}] ([{1}], [{2}]) VALUES (@Key, @Value)";

        private const string CreateDatabaseQuery = @"IF NOT EXISTS(SELECT name FROM master.dbo.sysdatabases WHERE '[' + name + ']' = N'{0}' OR name = N'{0}')
                                                        CREATE DATABASE [{0}] ON PRIMARY (NAME = '{0}', FILENAME = '{1}')";

        private const string CreateTableQuery = @"IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}')
                                                    CREATE TABLE [{0}] (
                                                        {1} nvarchar(50) NOT NULL,
                                                        {2} {3}{4},
                                                        UNIQUE({1})
                                                    )";

        #endregion

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
            string sql = String.Format(CreateDatabaseQuery, _dbName, filePath);
            ExecuteCommand(sql);
        }

        private void CreateTable()
        {
            string sql = String.Format(CreateTableQuery,
                            _tableName, _keyColumn, _valueColumn, _valueDbType.ToString(), 
                            String.IsNullOrEmpty(_valueDbTypeSizeLimit) ? String.Empty : "(" + _valueDbTypeSizeLimit + ")");
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

        public TValue ReadValue<TValue>(TKey key, TValue defaultValue = default(TValue))
        {
            TValue result = defaultValue;
            string sql = String.Format(SelectQuery, _valueColumn, _tableName, _keyColumn);

            ExecuteQuery(sql, 
                command =>
                {
                    command.Parameters.AddWithValue(KeyParam, key.ToString());
                }, 
                sqlReader => 
                {
                    if (sqlReader.Read())
                    {
                        result = (TValue)_serializer.Deserialize(sqlReader[0]);
                    }
                });

            return result;
        }

        public IDictionary<TKey, TValue> ReadMany<TValue>(IEnumerable<TKey> keys)
        {
            throw new NotImplementedException();
        }

        public bool WriteValue<TValue>(TKey key, TValue value)
        {
            string commandSql = String.Format(UpdateQuery, _tableName, _keyColumn, _valueColumn);

            int affectedRows = ExecuteCommandInTransaction(commandSql, command =>
                {
                    command.Parameters.AddWithValue(KeyParam, key.ToString());
                    command.Parameters.AddWithValue(ValueParam, _serializer.Serialize(value));
                });

            return affectedRows > 0;
        }

        public bool WriteMany<TValue>(IDictionary<TKey, TValue> values)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
