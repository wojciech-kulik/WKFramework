using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WKFramework.UnitTests.SettingsTests.TestClasses
{
    [Serializable]
    public class BasePerson
    {
        public string City { get; set; }
    }

    [Serializable]
    public class Person : BasePerson
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime DateOfBirth { get; set; }

        public int Height { get; set; }

        public string PhoneNumber { get; set; }

        public Car Car { get; set; }

        private string Address { get; set; }

        public void SetAddress(string address)
        {
            Address = address;
        }
    }

    [Serializable]
    public class Car
    {
        public string Model { get; set; }

        public DateTime Year { get; set; }
    }
}
