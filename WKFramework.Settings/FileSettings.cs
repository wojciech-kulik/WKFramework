using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WKFramework.Utils;
using WKFramework.Utils.Serializer;

namespace WKFramework.Settings
{
    public class FileSettings : ISettings
    {
        protected string _filePath;
        protected ISerializer<byte[]> _serializer = new GZipSerializer();
        protected Dictionary<object, object> _settings = new Dictionary<object, object>();
        protected Func<object, object> _keyConversion = x => x;

        public bool AutoSave { get; set; }

        #region Constructors

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

        #endregion

        #region Private methods

        private bool CanSetProperty(object obj, PropertyInfo property)
        {
            var setter = property.GetSetMethod(true);
            return setter != null && (obj != null || setter.IsStatic);
        }

        private bool CanGetProperty(object obj, PropertyInfo property)
        {
            var getter = property.GetGetMethod(true);
            return getter != null && (obj != null || getter.IsStatic);
        }

        private void TryAutoSave()
        {
            if (AutoSave)
                Save();
        }

        private string GetKeyFromProperty(PropertyInfo property)
        {
            return String.Format("{0}.{1}", property.DeclaringType.Name, property.Name);
        }

        private void ForEachProperty(Type type, Func<PropertyInfo, bool> filter, Action<PropertyInfo> action)
        {
            var properties = type.GetProperties().Where(x => !x.IsDefined(typeof(NonSerializedPropertyAttribute), false)).Where(filter);
            foreach (var prop in properties)
            {
                action(prop);
            }
        }

        private void ValidateKey(object key)
        {
            if (key == null)
                throw new ArgumentNullException("key");
        }

        private void ValidateKeys(IEnumerable keys)
        {
            foreach (var key in keys)
            {
                if (key == null)
                    throw new ArgumentNullException("key");
            }
        }

        #endregion

        public void SetKeyConversion(Func<object, object> conversion)
        {
            _keyConversion = conversion ?? (x => x);
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
                _settings = new Dictionary<object, object>();
                return false;
            }

            var bytes = File.ReadAllBytes(filePath);
            _settings = _serializer.Deserialize<Dictionary<object, object>>(bytes);

            return true;
        }

        public object ReadValue(object key, object defaultValue = null)
        {
            return ReadValue<object>(key, defaultValue);
        }

        public TValue ReadValue<TValue>(object key, TValue defaultValue = default(TValue))
        {
            ValidateKey(key);
            key = _keyConversion(key);

            if (!_settings.ContainsKey(key))
                return defaultValue;

            return (TValue)_settings[key].DeepClone();
        }

        public IDictionary<object, object> ReadMany(ICollection keys)
        {
            return ReadMany<object>(keys);
        }

        public IDictionary<object, TValue> ReadMany<TValue>(ICollection keys)
        {
            ValidateKeys(keys);

            var result = new Dictionary<object, TValue>();
            foreach(var key in keys)
            {
                if (_settings.ContainsKey(_keyConversion(key)))
                    result.Add(key, ReadValue<TValue>(key));
            }

            return result;
        }

        public IDictionary<object, object> ReadAll()
        {
            return ReadAll<object>();
        }

        public IDictionary<object, TValue> ReadAll<TValue>()
        {
            var result = new Dictionary<object, TValue>();
            foreach (var key in _settings.Keys)
            {
                result.Add(key, (TValue)_settings[key].DeepClone());
            }
            return result;
        }

        public bool WriteValue(object key, object value)
        {
            ValidateKey(key);
            WriteSingle(key, value);
            TryAutoSave();
            return true;
        }

        protected void WriteSingle(object key, object value)
        {
            key = _keyConversion(key);
            _settings[key] = value;
        }

        public bool WriteMany(IDictionary<object, object> values)
        {
            ValidateKeys(values.Keys);
            foreach (var key in values.Keys)
            {
                WriteSingle(key, values[key]);
            }
            TryAutoSave();
            return true;
        }

        public bool Remove(object key)
        {
            ValidateKey(key);
            bool result = RemoveSingle(key);
            TryAutoSave();

            return result;
        }

        protected bool RemoveSingle(object key)
        {
            key = _keyConversion(key);
            if (!_settings.ContainsKey(key))
                return false;

            return _settings.Remove(key);
        }

        public bool RemoveMany(ICollection keys)
        {
            ValidateKeys(keys);

            bool result = true;
            foreach (var key in keys)
            {
                if (!RemoveSingle(key))
                    result = false;
            }

            TryAutoSave();
            return result;
        }

        public void RemoveAll()
        {
            _settings.Clear();
            TryAutoSave();
        }

        public void RemoveProperties(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            RemovePropertiesOfType(obj.GetType());
        }

        public void RemoveProperties(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            RemovePropertiesOfType(type);
        }

        protected void RemovePropertiesOfType(Type type)
        {
            ForEachProperty(type, 
                x => true,
                prop =>
                {
                    var key = GetKeyFromProperty(prop);
                    if (_settings.ContainsKey(key))
                        _settings.Remove(key);
                });
            TryAutoSave();
        }

        public void ReadProperties(object destination)
        {
            if (destination == null)
                throw new ArgumentNullException("destination");

            ReadPropertiesOfType(destination, destination.GetType());
        }

        public void ReadStaticProperties(Type destinationType)
        {
            if (destinationType == null)
                throw new ArgumentNullException("destinationType");

            ReadPropertiesOfType(null, destinationType);
        }

        protected void ReadPropertiesOfType(object destination, Type type)
        {
            ForEachProperty(type, 
                x => CanSetProperty(destination, x),
                prop =>
                {
                    var key = GetKeyFromProperty(prop);
                    if (_settings.ContainsKey(key))
                        prop.SetValue(destination, _settings[key], null);
                });
        }

        public bool WriteProperties(object source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            return WritePropertiesOfType(source, source.GetType());
        }

        public bool WriteStaticProperties(Type sourceType)
        {
            if (sourceType == null)
                throw new ArgumentNullException("sourceType");

            return WritePropertiesOfType(null, sourceType);
        }

        protected bool WritePropertiesOfType(object source, Type type)
        {
            ForEachProperty(type, x => CanGetProperty(source, x), prop => _settings[GetKeyFromProperty(prop)] = prop.GetValue(source, null));
            TryAutoSave();
            return true;
        }
    }
}
