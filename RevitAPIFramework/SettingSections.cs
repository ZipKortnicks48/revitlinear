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
        public string op = "ОП";
        public void SettingSectionsFromForm(string _ark, string a, string b, string c, string d, string _op)
        {
            ark = _ark;
            mark = a;
            section =  b + "x" + c + "x" + d;
            op=_op;
        }
        public void SettingSectionsFromFile(string _ark, string _mark, string _section)
        {
            ark = _ark;
            mark = _mark;
            section = _section;
        }
      public  string GetStringForFile()
        {
            return ark + "&" + op +"&"+mark + ";" + section;
        }
        public string GetStrForDrawing() {
            return mark + " " + section;
        }
    }
}
