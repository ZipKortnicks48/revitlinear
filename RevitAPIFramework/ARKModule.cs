using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPIFramework
{
   public class ARKModule
    {
        public string id;
        public int index;
        public int ARKNumber;
        public FamilyInstance revitModule;
        public FamilySymbol revitSymbol;
        public List<MEPSystem> systems;
        public FamilyInstance equipment;
        public string filename;
        public string filepath;
        public string mark;
        public int countShleifs;
        private TableCreator tableCreator;
        private ViewDrafting vd;
        public List<MEPSystem> alertSystems;
        public ARKModule(string _id, int _index, FamilyInstance _model)
        {
            id = _id;
            index = _index;
            revitModule = _model;
            equipment = _model;
            systems = new List<MEPSystem>();
            alertSystems = new List<MEPSystem>();

        }
        public string getFullName()
        {
            return "(" + equipment.LookupParameter("Марка").AsString() + ") " + revitModule.Name;
        }
        public void addSystem(MEPSystem s)
        {
            systems.Add(s);            
        }
        public void addAlertSystem(MEPSystem s)
        {
            alertSystems.Add(s);
        }
        public void addFilename(string s)
        {
            filename = s;
        }
        public void addFilepath(string s)
        {
            filepath = s;
        }
        public int getCountShleifs()
        {
            return countShleifs;
        }
        public void setVD(ViewDrafting v)
        {
            vd = v;
        }
        public ViewDrafting getVD()
        {
            return vd;
        }
        public void createTable(Document doc, XYZ start)
        {
            tableCreator = new TableCreator(this,start);
            tableCreator.CreateTable(doc);
        }
      
    }
}
