using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPIFramework
{
   public class ARKModule
    {
        public string id;
        public int index;
        FamilyInstance revitModule;
        List<MEPSystem> systems;
        public string filename;
        public string filepath;
        public ARKModule(string _id, int _index, FamilyInstance _model)
        {
            id = _id;
            index = _index;
            revitModule = _model;
            systems = new List<MEPSystem>();
        }
        public string getFullName()
        {
            return id + " " + revitModule.Name;
        }
        public void addSystem(MEPSystem s)
        {
            systems.Add(s);            
        }
        public void addFilename(string s)
        {
            filename = s;
        }
        public void addFilepath(string s)
        {
            filepath = s;
        }
    }
}
