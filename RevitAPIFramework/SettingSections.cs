using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPIFramework
{
    public class SettingSections
    {
        public string ark="";
        public string mark="";
        public string section="";
        
        public void SettingSectionsFromForm(string _ark, string a, string b, string c, string d)
        {
            ark = _ark;
            mark = a;
            section =  b + "x" + c + "x" + d;
        }
        public void SettingSectionsFromFile(string _ark, string _mark, string _section)
        {
            ark = _ark;
            mark = _mark;
            section = _section;
        }
      public  string GetStringForFile()
        {
            return ark + "&" + mark + ";" + section;
        }
        public string GetStrForDrawing() {
            return mark + " " + section;
        }
    }
}
