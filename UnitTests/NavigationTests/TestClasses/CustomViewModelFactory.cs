using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTests.NavigationTests.Mocks;
using WKFramework.WPF.Navigation;

namespace UnitTests.NavigationTests
{
    public class CustomViewModelFactory : ViewModelFactory
    {
        public override T Get<T>()
        {
            return (T)Get(typeof(T));
        }

        public override object Get(Type type)
        {
            var result = base.Get(type);

            if (result is PersonViewModel)
            {
                (result as PersonViewModel).LastName = "[PVW] Changed by factory";
            }
            else if (result is PersonDetailsViewModel)
            {
                (result as PersonDetailsViewModel).City = "[PDVW] Changed by factory";
            }

            return result;
        }
    }
}
