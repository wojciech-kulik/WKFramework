using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WKFramework.WPF.Navigation
{
    public interface IViewModelFactory
    {
        T Get<T>();

        object Get(Type type);
    }
}
