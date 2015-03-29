using System.Reflection;
using WKFramework.WPF.Navigation;

namespace UnitTests.NavigationTests.Mocks
{
    public class WindowManagerMock : WindowManager
    {
        public WindowManagerMock()
        {
        }

        public WindowManagerMock(IViewModelFactory factory)
            : base(factory)
        { }

        protected override Assembly[] GetAssembliesWithViews()
        {
            return new Assembly[] { Assembly.GetExecutingAssembly() };
        }

        public Assembly[] GetBaseAssembliesWithViews()
        {
            return base.GetAssembliesWithViews();
        }
    }
}
