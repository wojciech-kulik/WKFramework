using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace WKFramework.WPF.Navigation
{
    public interface INavigationService<TVM>
    {
        INavigationService<TVM> WithParam<TProperty>(Expression<Func<TVM, TProperty>> property, TProperty value);

        INavigationService<TVM> SetupViewModel(Action<TVM> action);

        INavigationService<TVM> DoIfAccepted(Action<TVM> action);

        INavigationService<TVM> DoIfCancelled(Action<TVM> action);

        INavigationService<TVM> DoAfterClose(Action<TVM> action);

        void ShowWindow();

        bool ShowDialog();
    }



    public interface INavigationService
    {
        INavigationService<TViewModel> GetWindow<TViewModel>();
    }
}
