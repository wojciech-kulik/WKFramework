using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WKFramework.Settings.Targets
{
    public interface ISettingsTarget<K> : IDisposable
    {
        V ReadValue<V>(K key, V defaultValue = default(V));

        IDictionary<K, V> ReadMany<V>(IEnumerable<K> keys);

        bool WriteValue<V>(K key, V value);

        bool WriteMany<V>(IDictionary<K, V> values);
    }
}
