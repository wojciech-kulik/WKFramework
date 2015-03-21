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
        public const int MaxKeyLength = 50;

        protected readonly string _tableName;
        protected readonly string _keyColumn = "optionName";
        protected readonly string _valueColumn = "value";
        protected readonly SqlDbType _valueDbType = SqlDbType.VarBinary;
        protected readonly string _valueDbTypeSizeLimit = "MAX";

        protected string _dbName;
        protected string _connectionString;
        
        protected ISerializer _serializer = new BinarySerializer();
        protected Func<TKey, string> _keyConversion = x => x.ToString();

        public MsSqlServerTarget(string connectionString, string tableName, string dbName = null)
        {
            _connectionString = connectionString;
            _tableName = tableName;
            _dbName = dbName;

            InitializeDatabase();
        }

        public MsSqlServerTarget(string connectionString, string tableName, string dbName,
                                 SqlDbType valueDbType, string valueDbTypeSizeLimit, ISerializer valueSerializer)
        {
            _connectionString = connectionString;
            _tableName = tableName;
            _dbName = dbName;
            _valueDbType = valueDbType;
            _valueDbTypeSizeLimit = valueDbTypeSizeLimit;
            _serializer = valueSerializer;

            InitializeDatabase();
        }

        #region SQL Queries

        private const string KeyParam = "@Key";

        private const string KeyToDelParam = "@KeyToDel";

        private const string ValueParam = "@Value";

        private const string SelectQuery = "SELECT [{0}] FROM [{1}] WHERE [{2}] = @Key";

        private const string SelectManyQuery = "SELECT [{2}], [{0}] FROM [{1}] WHERE [{2}] in ({3})";

        private const string SelectAllQuery = "SELECT [{0}], [{1}] FROM [{2}]";

        private const string UpdateQuery = "UPDATE [{0}] SET [{2}]=@Value WHERE [{1}]=@Key \r\n" +
                                           " IF @@ROWCOUNT = 0 \r\n" +
                                           "    INSERT INTO [{0}] ([{1}], [{2}]) VALUES (@Key, @Value)";

        private const string UpdateManyQuery = "DELETE FROM [{0}] WHERE [{1}] in ({3}); \r\n" +
                                               "INSERT INTO [{0}] ([{1}], [{2}]) VALUES {4};";

        private const string DeleteQuery = "DELETE FROM [{0}] WHERE [{1}] = @Key"; 

        private const string DeleteManyQuery = "DELETE FROM [{0}] WHERE [{1}] in ({2})";

        private const string DeleteAllQuery = "DELETE FROM [{0}]";

        private const string CreateDatabaseQuery = "IF NOT EXISTS(SELECT name FROM master.dbo.sysdatabases WHERE '[' + name + ']' = N'{0}' OR name = N'{0}') \r\n" +
                                                   "   CREATE DATABASE [{0}] ON PRIMARY (NAME = '{0}', FILENAME = '{1}')";

        private const string CreateTableQuery = "IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}') \r\n" +
                                                "   CREATE TABLE [{0}] ( \r\n" +
                                                "       {1} nvarchar(50) NOT NULL, \r\n" +
                                                "       {2} {3}{4}, \r\n" +
                                                "       UNIQUE({1}) \r\n" +
                                                "    )";

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

        private int ExecuteCommand(string sql, Action<SqlCommand> commandAction = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                connection.Open();

                if (commandAction != null)
                    commandAction(command);

                return command.ExecuteNonQuery();
            }
        }

        private void ExecuteQuery(string sql, Action<SqlCommand> commandAction, Action<SqlDataReader> resultAction)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                connection.Open();

                if (commandAction != null)
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

        protected string ConvertKey(TKey key)
        {
            var strKey = _keyConversion(key);
            if (strKey.Length > MaxKeyLength)
            {
                throw new ArgumentOutOfRangeException("key", String.Format("Key cannot be longer than {0} characters. Default key conversion invokes ToString.", MaxKeyLength));
            }

            return strKey;
        }

        public void SetKeyConversion(Func<TKey, string> conversion)
        {
            _keyConversion = conversion ?? (x => x.ToString());
        }

        public object ReadValue(TKey key, object defaultValue = null)
        {
            return ReadValue<object>(key, defaultValue);
        }

        public TValue ReadValue<TValue>(TKey key, TValue defaultValue = default(TValue))
        {
            TValue result = defaultValue;
            string sql = String.Format(SelectQuery, _valueColumn, _tableName, _keyColumn);

            ExecuteQuery(sql, 
                command =>
                {
                    command.Parameters.AddWithValue(KeyParam, ConvertKey(key));
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

        private string PrepareReadManySQL(int count)
        {
            StringBuilder listOfParams = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                listOfParams.Append(KeyParam);
                listOfParams.Append(i);
                if (i < count - 1)
                    listOfParams.Append(", ");
            }

            return String.Format(SelectManyQuery, _valueColumn, _tableName, _keyColumn, listOfParams.ToString());
        }

        public IDictionary<TKey, object> ReadMany(IEnumerable<TKey> keys)
        {
            return ReadMany<object>(keys);
        }

        public IDictionary<TKey, TValue> ReadMany<TValue>(IEnumerable<TKey> keys)
        {
            if (keys.Count() == 0)
                return new Dictionary<TKey, TValue>();

            var result = new Dictionary<TKey, TValue>();
            var keysDictionary = new Dictionary<string, TKey>();
            foreach (var key in keys)
            {
                keysDictionary.Add(ConvertKey(key), key);
            }

            ExecuteQuery(
                PrepareReadManySQL(keys.Count()),
                command =>
                {
                    int i = 0;
                    foreach (var key in keys)
                    {
                        command.Parameters.AddWithValue(KeyParam + i.ToString(), ConvertKey(key));
                        i++;
                    }
                },
                sqlReader =>
                {
                    while (sqlReader.Read())
                    {
                        var key = keysDictionary[(string)sqlReader[0]];
                        var value = (TValue)_serializer.Deserialize(sqlReader[1]);
                        result.Add(key, value);
                    }
                });

            return result;
        }

        public IDictionary<string, object> ReadAll()
        {
            return ReadAll<object>();
        }

        public IDictionary<string, TValue> ReadAll<TValue>()
        {
            var result = new Dictionary<string, TValue>();
            var sql = String.Format(SelectAllQuery, _keyColumn, _valueColumn, _tableName);

            ExecuteQuery(sql, null,
                sqlReader =>
                {
                    while (sqlReader.Read())
                    {
                        var key = (string)sqlReader[0];
                        var value = (TValue)_serializer.Deserialize(sqlReader[1]);
                        result.Add(key, value);
                    }
                });

            return result;
        }

        public bool WriteValue(TKey key, object value)
        {
            string commandSql = String.Format(UpdateQuery, _tableName, _keyColumn, _valueColumn);

            int affectedRows = ExecuteCommandInTransaction(commandSql, command =>
                {
                    command.Parameters.AddWithValue(KeyParam, ConvertKey(key));
                    command.Parameters.AddWithValue(ValueParam, _serializer.Serialize(value));
                });

            return affectedRows > 0;
        }

        private string PrepareWriteManySQL(int count)
        {
            StringBuilder keys = new StringBuilder();
            StringBuilder valuesToInsert = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                keys.Append(KeyToDelParam);
                keys.Append(i);
                if (i < count - 1)
                    keys.Append(", ");

                valuesToInsert.Append("(");
                valuesToInsert.Append(KeyParam);
                valuesToInsert.Append(i);
                valuesToInsert.Append(", ");
                valuesToInsert.Append(ValueParam);
                valuesToInsert.Append(i);
                valuesToInsert.Append(")");
                if (i < count - 1)
                    valuesToInsert.Append(", ");
            }

            return String.Format(UpdateManyQuery, _tableName, _keyColumn, _valueColumn, keys.ToString(), valuesToInsert.ToString());
        }

        public bool WriteMany(IDictionary<TKey, object> values)
        {
            if (values.Count() == 0)
                return true;

            var sql = PrepareWriteManySQL(values.Count);

            int affectedRows = ExecuteCommandInTransaction(sql, command =>
            {
                int i = 0;
                foreach (var item in values)
                {
                    command.Parameters.AddWithValue(KeyToDelParam + i.ToString(), ConvertKey(item.Key));
                    command.Parameters.AddWithValue(KeyParam + i.ToString(), ConvertKey(item.Key));
                    command.Parameters.AddWithValue(ValueParam + i.ToString(), _serializer.Serialize(item.Value));

                    i++;
                }
            });

            return affectedRows == values.Count;
        }

        public bool RemoveValue(TKey key)
        {
            string commandSql = String.Format(DeleteQuery, _tableName, _keyColumn);

            int affectedRows = ExecuteCommand(commandSql, command =>
            {
                command.Parameters.AddWithValue(KeyParam, ConvertKey(key));
            });

            return affectedRows > 0;
        }

        private string PrepareRemoveManySQL(int count)
        {
            StringBuilder listOfParams = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                listOfParams.Append(KeyParam);
                listOfParams.Append(i);
                if (i < count - 1)
                    listOfParams.Append(", ");
            }

            return String.Format(DeleteManyQuery, _tableName, _keyColumn, listOfParams.ToString());
        }

        public bool RemoveMany(IEnumerable<TKey> keys)
        {
            if (keys.Count() == 0)
                return true;

            int affectedRows = ExecuteCommand(PrepareRemoveManySQL(keys.Count()), command =>
            {
                int i = 0;
                foreach (var key in keys)
                {
                    command.Parameters.AddWithValue(KeyParam + i.ToString(), ConvertKey(key));
                    i++;
                }
            });

            return affectedRows == keys.Count();
        }

        public void RemoveAll()
        {
            ExecuteCommand(String.Format(DeleteAllQuery, _tableName));
        }
    }
}
