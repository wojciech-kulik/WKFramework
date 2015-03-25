using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WKFramework.WPF.Navigation
{
    public class NavigationService : INavigationService
    {
        IWindowManager _windowManager;
        IViewModelFactory _vmFactory = new ViewModelFactory();

        public NavigationService()
        {
            _windowManager = new WindowManager(_vmFactory);
        }

        public NavigationService(IViewModelFactory viewModelFactory)
        {
            _vmFactory = viewModelFactory;
            _windowManager = new WindowManager(viewModelFactory);
        }

        public NavigationService(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        public NavigationService(IViewModelFactory viewModelFactory, IWindowManager windowManager)
        {
            _windowManager = windowManager;
            _vmFactory = viewModelFactory;
        }

        public INavigationService<TViewModel> GetWindow<TViewModel>()
        {
            return new NavigationService<TViewModel>(_windowManager, _vmFactory.Get<TViewModel>());
        }
    }



    public class NavigationService<TVM> : INavigationService<TVM>
    {
        IWindowManager _windowManager;
        TVM _viewModel;
        Action _action;

        public NavigationService(IWindowManager windowManager, TVM viewModel)
        {
            _windowManager = windowManager;
            _viewModel = viewModel;
        }

        public INavigationService<TVM> WithParam<TProperty>(Expression<Func<TVM, TProperty>> property, TProperty value)
        {
            var prop = (PropertyInfo)((MemberExpression)property.Body).Member;
            prop.SetValue(_viewModel, value, null);

            return this;
        }

        public INavigationService<TVM> DoBeforeShow(Action<TVM> action)
        {
            action(_viewModel);
            return this;
        }

        public INavigationService<TVM> DoIfSuccess(Action action)
        {
            _action = action;
            return this;
        }

        public void ShowWindow()
        {
            _windowManager.ShowWindow(_viewModel);
        }

        public bool ShowWindowModal()
        {
            bool result = _windowManager.ShowDialog(_viewModel) ?? false;
            if (result && _action != null)
                _action();

            return result;
        }
    }
}
