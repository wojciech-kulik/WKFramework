using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WKFramework.WPF.Navigation
{
    public class WindowManager : IWindowManager
    {
        private bool _isInitialized = false;
        protected string ViewFileSuffix = "View";
        protected string ViewModelFileSuffix = "ViewModel";
        protected Dictionary<string, Type> _windows = new Dictionary<string, Type>();
        IViewModelFactory _vmFactory = new ViewModelFactory();

        public WindowManager()
        {
            Initialize();
        }

        public WindowManager(IViewModelFactory viewModelFactory)
        {
            _vmFactory = viewModelFactory;
            Initialize();
        }

        #region Initialization

        protected virtual void Initialize()
        {
            if (_isInitialized)
                return;

            var types = GetAssemblyWithViews().GetTypes().Where(x => typeof(Window).IsAssignableFrom(x));
            foreach (var type in types)
            {
                var name = ExtractName(type, ViewFileSuffix);
                _windows[name] = type;
            }

            _isInitialized = true;
        }

        protected virtual Assembly GetAssemblyWithViews()
        {
            return Assembly.GetEntryAssembly();
        }

        #endregion

        #region Private methods

        private static string ExtractName(Type type, string ending)
        {
            var name = type.Name;
            var viewModelPos = name.LastIndexOf(ending, StringComparison.InvariantCultureIgnoreCase);
            if (viewModelPos != -1)
            {
                name = name.Substring(0, viewModelPos);
            }

            return name;
        }

        private Window CreateWindow(Type type, object viewModel)
        {
            var window = (Window)Activator.CreateInstance(type);
            window.DataContext = viewModel;
            return window;
        }

        private void ThrowNoCorrespondingView(object viewModel)
        {
            throw new ArgumentOutOfRangeException("There is no corresponding View (window) for {0} ViewModel. " +
                                                  "Make sure you use the correct naming convention.",
                                                  viewModel.GetType().FullName);
        }

        #endregion

        #region IWindowManager

        public Dictionary<string, Type> KnownWindows
        {
            get
            {
                return _windows;
            }
        }

        public void ShowWindow<TViewModel>()
        {
            var vm = _vmFactory.Get<TViewModel>();
            ShowWindow(vm);
        }

        public bool? ShowDialog<TViewModel>()
        {
            var vm = _vmFactory.Get<TViewModel>();
            return ShowDialog(vm);
        }

        public void ShowWindow(object viewModel)
        {
            Initialize();

            var name = ExtractName(viewModel.GetType(), ViewModelFileSuffix);
            if (_windows.ContainsKey(name))
            {
                var window = CreateWindow(_windows[name], viewModel);
                window.Show();
            }
            else
            {
                ThrowNoCorrespondingView(viewModel);
            }
        }

        public bool? ShowDialog(object viewModel)
        {
            Initialize();

            var name = ExtractName(viewModel.GetType(), ViewModelFileSuffix);
            if (_windows.ContainsKey(name))
            {
                var window = CreateWindow(_windows[name], viewModel);
                return window.ShowDialog();
            }
            else
            {
                ThrowNoCorrespondingView(viewModel);
            }

            return null;
        }

        #endregion
    }
}
