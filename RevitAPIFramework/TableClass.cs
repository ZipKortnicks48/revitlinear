using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPIFramework
{
    class TableClass
    {
        public List<TableElement> elements;
        public TableClass()
        {
            elements = new List<TableElement>();
        }
        public void addElement(string name, string pos)
        {
            if (elements.Count == 0) { elements.Add(new TableElement(name,pos)); return; }
            foreach(TableElement elem in elements)
            {
                if (elem.itIs(name)) { elem.IncrementCount(); return; } 
            }
            elements.Add(new TableElement(name,pos));
        }
    }
    class TableElement
    {
        private string name = "";
        private int count = 0;
        private string position = "";
        public TableElement(string _name, string _pos)
        {
            this.name = _name;
            this.position = _pos;
            count++;
        }
        public void IncrementCount()
        {
            count++;
        }
        public string getName()
        {
            return name;
        }
        public string getPosition()
        {
            return position;
        }
        public string getCount() {

            return count.ToString();
        }
        public bool itIs(string s)
        {
            if (s == name) { return true; } else { return false; }
        }
    }
}
