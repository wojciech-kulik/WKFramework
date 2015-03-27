using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTests.SettingsTests.TestClasses;

namespace UnitTests.TestClasses
{
    [Serializable]
    public class Item
    {
        public string Name { get; set; }
    }

    [Serializable]
    public class Furniture
    {
        public int Height { get; set; }
    }

    [Serializable]
    public class Person
    {
        public string FullName { get; set; }
    }

    [Serializable]
    public class Shelf : Furniture
    {
        public int Capacity { get; set; }

        public IList<Item> Items { get; set; }

        public Person Owner { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as Shelf;

            if (other == null || Capacity != other.Capacity || Owner.FullName != other.Owner.FullName)
                return false;

            if (Items != null && other.Items != null)
            {
                if (Items.Count != other.Items.Count)
                    return false;
            }
            else if (Items == null || other.Items == null)
            {
                return false;
            }

            foreach(var item in Items)
            {
                if (!other.Items.Any(x => x.Name == item.Name))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + Capacity.GetHashCode();
                if (Items != null)
                    hash = hash * 23 + Items.GetHashCode();
                if (Owner != null)
                    hash = hash * 23 + Owner.GetHashCode();
                return hash;
            }
        }
    }
}
