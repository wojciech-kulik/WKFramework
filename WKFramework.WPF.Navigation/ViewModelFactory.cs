using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WKFramework.WPF.Navigation
{
    public class ViewModelFactory : IViewModelFactory
    {
        public virtual T Get<T>()
        {
            return Activator.CreateInstance<T>();
        }

        public virtual object Get(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }
}
