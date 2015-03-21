using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WKFramework.Settings.Targets
{
    public interface ISettingsTarget<TKey>
    {
        object ReadValue(TKey key, object defaultValue = null);

        TValue ReadValue<TValue>(TKey key, TValue defaultValue = default(TValue));

        IDictionary<TKey, object> ReadMany(IEnumerable<TKey> keys);

        IDictionary<TKey, TValue> ReadMany<TValue>(IEnumerable<TKey> keys);

        IDictionary<string, object> ReadAll();

        IDictionary<string, TValue> ReadAll<TValue>();

        bool WriteValue(TKey key, object value);

        bool WriteMany(IDictionary<TKey, object> values);

        bool RemoveValue(TKey key);

        bool RemoveMany(IEnumerable<TKey> keys);

        void RemoveAll();

        void SetKeyConversion(Func<TKey, string> conversion);
    }
}
