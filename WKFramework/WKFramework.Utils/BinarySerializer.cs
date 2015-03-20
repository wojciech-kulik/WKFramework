using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace WKFramework.Utils
{
    public class BinarySerializer
    {
        private Dictionary<Type, IBinaryConverter> _converters = new Dictionary<Type, IBinaryConverter>();

        public void RegisterValueConverter(Type type, IBinaryConverter converter)
        {
            _converters[type] = converter;
        }

        public void UnregisterValueConverter(Type type)
        {
            if (_converters.ContainsKey(type))
                _converters.Remove(type);
        }

        public byte[] ConvertToBinary<T>(T obj)
        {
            var type = typeof(T);
            if (_converters.ContainsKey(type))
            {
                return _converters[type].ConvertToBinary(obj);
            }
            else
            {
                IFormatter formatter = new BinaryFormatter();
                using (var stream = new MemoryStream())
                {
                    formatter.Serialize(stream, obj);
                    stream.Seek(0, SeekOrigin.Begin);

                    var bytes = new byte[stream.Length];
                    int read = stream.Read(bytes, 0, (int)stream.Length);

                    if (read != stream.Length)
                        throw new InvalidOperationException("Couldn't serialize whole object.");

                    return bytes;
                }
            }
        }

        public T ConvertFromBinary<T>(byte[] data)
        {
            var type = typeof(T);
            if (_converters.ContainsKey(type))
            {
                return (T)_converters[type].ConvertFromBinary(data);
            }
            else
            {
                IFormatter formatter = new BinaryFormatter();
                using (var stream = new MemoryStream())
                {
                    stream.Write(data, 0, data.Length);
                    stream.Seek(0, SeekOrigin.Begin);

                    return (T)formatter.Deserialize(stream);
                }
            }
        }
    }
}
