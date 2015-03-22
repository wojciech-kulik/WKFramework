﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WKFramework.UnitTests.SettingsTests.TestClasses
{
    public class BasePerson
    {
        public string City { get; set; }
    }

    public class Person : BasePerson
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime DateOfBirth { get; set; }

        public int Height { get; set; }

        public string PhoneNumber { get; set; }

        private string Address { get; set; }

        public void SetAddress(string address)
        {
            Address = address;
        }
    }
}