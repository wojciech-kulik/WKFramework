using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WKFramework.Utils.Serializer
{
    public abstract class Serializer<TSerialized> : ISerializer<TSerialized>, ISerializer
    {
        protected Dictionary<Type, IConverter<TSerialized>> _converters = new Dictionary<Type, IConverter<TSerialized>>();

        public virtual void RegisterValueConverter(Type type, IConverter<TSerialized> converter)
        {
            _converters[type] = converter;
        }

        public virtual void UnregisterValueConverter(Type type)
        {
            if (_converters.ContainsKey(type))
                _converters.Remove(type);
        }

        protected virtual bool TryCustomConvert<TSource>(TSource obj, out TSerialized result)
        {
            result = default(TSerialized);

            var type = typeof(TSource);
            if (_converters.ContainsKey(type))
            {
                result = _converters[type].Convert(obj);
                return true;
            }

            return false;
        }

        protected virtual bool TryCustomConvertFrom<TResult>(TSerialized obj, out TResult result)
        {
            result = default(TResult);

            var type = typeof(TSerialized);
            if (_converters.ContainsKey(type))
            {
                result = _converters[type].ConvertFrom<TResult>(obj);
                return true;
            }

            return false;
        }

        protected abstract TSerialized DoSerialize<TSource>(TSource obj);

        protected abstract TResult DoDeserialize<TResult>(TSerialized data);

        #region ISerializer<TSerialized>

        public virtual TSerialized Serialize<TSource>(TSource obj)
        {
            TSerialized result = default(TSerialized);

            if (!TryCustomConvert(obj, out result))
            {
                return DoSerialize(obj);
            }

            return result;
        }

        public virtual TResult Deserialize<TResult>(TSerialized data)
        {
            TResult result = default(TResult);

            if (!TryCustomConvertFrom(data, out result))
            {
                return DoDeserialize<TResult>(data);
            }

            return result;
        }

        #endregion

        #region ISerializer

        object ISerializer.Serialize(object obj)
        {
            return Serialize(obj);
        }

        object ISerializer.Deserialize(object obj)
        {
            return Deserialize<object>((TSerialized)obj);
        }

        #endregion
    }
}
