using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UnitTests.NavigationTests.Mocks
{
    public class PersonView : TestWindow { }

    public class PersonDetailsView : TestWindow { }




    public class PersonViewModel
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
    }

    public class PersonDetailsViewModel
    {
        public string Address { get; set; }

        public string City { get; set; }

        public int Age { get; set; }
    }

    public class ContactsViewModel
    {
    }
}
