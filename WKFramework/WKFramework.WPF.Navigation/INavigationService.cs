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

        INavigationService<TVM> DoIfSuccess(Action action);

        INavigationService<TVM> DoBeforeShow(Action<TVM> action);

        void ShowWindow();

        bool ShowWindowModal();
    }



    public interface INavigationService
    {
        INavigationService<TViewModel> GetWindow<TViewModel>();
    }
}
