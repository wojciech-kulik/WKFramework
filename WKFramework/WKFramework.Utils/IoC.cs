using System;
using System.Linq;
using System.Collections.Generic;

namespace WKFramework.WPF.Navigation 
{
    public static class IoC 
    {
        private static SimpleContainer _container = new SimpleContainer();

        /// <summary>
        /// Gets an instance from the container.
        /// </summary>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <returns>The resolved instance.</returns>
        public static T Get<T>() 
        {
            return (T)_container.GetInstance(typeof(T), null);
        }

        /// <summary>
        /// Returns IoC container used in this class.
        /// </summary>
        public static SimpleContainer Container
        {
            get
            {
                return _container;
            }
        }
    }
}
