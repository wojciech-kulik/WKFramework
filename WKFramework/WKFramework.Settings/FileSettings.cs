using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WKFramework.Utils.Serializer;

namespace WKFramework.Settings
{
    public class FileSettings<TKey> : ISettings<TKey>
    {
        protected string _filePath;
        protected ISerializer<byte[]> _serializer = new BinarySerializer();
        protected Dictionary<TKey, object> _settings = new Dictionary<TKey, object>();

        public bool AutoSave { get; set; }

        public FileSettings()
        {
            AutoSave = true;
        }

        public FileSettings(string filePath)
        {
            AutoSave = true;
            Load(filePath);
        }

        public FileSettings(string filePath, ISerializer<byte[]> serializer, bool autoSave = true)
        {
            _serializer = serializer;
            AutoSave = autoSave;
            Load(filePath);
        }

        private void TryAutoSave()
        {
            if (AutoSave)
                Save();
        }

        public void Save()
        {
            var bytes = _serializer.Serialize(_settings);
            File.WriteAllBytes(_filePath, bytes);
        }

        public bool Load(string filePath)
        {
            _filePath = filePath;

            if (!File.Exists(filePath))
            {
                _settings = new Dictionary<TKey, object>();
                return false;
            }

            var bytes = File.ReadAllBytes(filePath);
            _settings = _serializer.Deserialize<Dictionary<TKey, object>>(bytes);

            return true;
        }

        public object ReadValue(TKey key, object defaultValue = null)
        {
            return ReadValue<object>(key, defaultValue);
        }

        public TValue ReadValue<TValue>(TKey key, TValue defaultValue = default(TValue))
        {
            if (key == null)
                throw new ArgumentNullException("key");
            if (!_settings.ContainsKey(key))
                return defaultValue;

            return (TValue)_settings[key].DeepClone();
        }

        public IDictionary<TKey, object> ReadMany(IEnumerable<TKey> keys)
        {
            return ReadMany<object>(keys);
        }

        public IDictionary<TKey, TValue> ReadMany<TValue>(IEnumerable<TKey> keys)
        {
            var result = new Dictionary<TKey, TValue>();
            foreach(var key in keys)
            {
                if (_settings.ContainsKey(key))
                    result.Add(key, ReadValue<TValue>(key));
            }

            return result;
        }

        public IDictionary<string, object> ReadAll()
        {
            return ReadAll<object>();
        }

        public IDictionary<string, TValue> ReadAll<TValue>()
        {
            var result = new Dictionary<string, TValue>();
            foreach (var key in _settings.Keys)
            {
                result.Add(key.ToString(), (TValue)_settings[key].DeepClone());
            }
            return result;
        }

        public bool WriteValue(TKey key, object value)
        {
            _settings[key] = value;
            TryAutoSave();
            return true;
        }

        public bool WriteMany(IDictionary<TKey, object> values)
        {
            foreach (var key in values.Keys)
            {
                WriteValue(key, values[key]);
            }
            return true;
        }

        public bool RemoveValue(TKey key)
        {
            throw new NotImplementedException();
        }

        public bool RemoveMany(IEnumerable<TKey> keys)
        {
            throw new NotImplementedException();
        }

        public void RemoveAll()
        {
            _settings.Clear();
            TryAutoSave();
        }

        public void RemoveProperties(object obj)
        {
            throw new NotImplementedException();
        }

        public void LoadProperties(object destination)
        {
            throw new NotImplementedException();
        }

        public bool SaveProperties(object source)
        {
            throw new NotImplementedException();
        }

        public void SetKeyConversion(Func<TKey, string> conversion)
        {
            throw new NotImplementedException();
        }
    }
}
