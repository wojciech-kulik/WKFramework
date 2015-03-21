using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WKFramework.Settings.Targets
{
    public interface ISettingsTarget<TKey> : IDisposable
    {
        object ReadValue(TKey key, object defaultValue = null);

        V ReadValue<V>(TKey key, V defaultValue = default(V));

        IDictionary<TKey, object> ReadMany(IEnumerable<TKey> keys);

        IDictionary<TKey, V> ReadMany<V>(IEnumerable<TKey> keys);

        bool WriteValue(TKey key, object value);

        bool WriteMany(IDictionary<TKey, object> values);

        void SetKeyConversion(Func<TKey, string> conversion);
    }
}
