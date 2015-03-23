using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WKFramework.Settings
{
    public interface ISettings
    {
        object ReadValue(object key, object defaultValue = null);

        TValue ReadValue<TValue>(object key, TValue defaultValue = default(TValue));

        IDictionary<object, object> ReadMany(ICollection keys);

        IDictionary<object, TValue> ReadMany<TValue>(ICollection keys);

        IDictionary<object, object> ReadAll();

        IDictionary<object, TValue> ReadAll<TValue>();

        bool WriteValue(object key, object value);

        bool WriteMany(IDictionary<object, object> values);

        bool Remove(object key);

        bool RemoveMany(ICollection keys);

        void RemoveAll();

        void RemoveProperties(object obj);

        void LoadProperties(object destination);

        bool SaveProperties(object source);

        void SetKeyConversion(Func<object, object> conversion);
    }
}
