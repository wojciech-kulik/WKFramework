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
        private IWindowManager _windowManager;
        private IViewModelFactory _vmFactory = new ViewModelFactory();

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
        private IWindowManager _windowManager;
        private TVM _viewModel;
        private Action<TVM> _successAction;
        private Action<TVM> _cancelAction;
        private Action<TVM> _closeAction;

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

        public INavigationService<TVM> SetupViewModel(Action<TVM> action)
        {
            if (action != null)
            {
                action(_viewModel);
            }
            return this;
        }

        public INavigationService<TVM> DoIfAccepted(Action<TVM> action)
        {
            _successAction = action;
            return this;
        }

        public INavigationService<TVM> DoIfCancelled(Action<TVM> action)
        {
            _cancelAction = action;
            return this;
        }

        public INavigationService<TVM> DoAfterClose(Action<TVM> action)
        {
            _closeAction = action;
            return this;
        }

        public void ShowWindow()
        {
            _windowManager.ShowWindow(_viewModel, _closeAction);
        }

        public bool ShowDialog()
        {
            bool result = _windowManager.ShowDialog(_viewModel) ?? false;

            if (_closeAction != null)
                _closeAction(_viewModel);

            if (result && _successAction != null)
            {
                _successAction(_viewModel);
            }
            else if (!result && _cancelAction != null)
            {
                _cancelAction(_viewModel);
            }

            return result;
        }
    }
}
