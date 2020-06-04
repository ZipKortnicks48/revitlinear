using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Electrical;
using System.Text;
using RevitAPIFramework;
using System.IO;
using System.Data.SqlTypes;
using System.Runtime.Remoting.Messaging;

namespace RevitAPIFramework
{
    
    public class TableCreator
    {
        Document doc;
        ARKModule module;
        XYZ start;
        TableClass table;
        public TableCreator(ARKModule _module, XYZ _start)
        {
            module = _module;
            start = _start;
            table = new TableClass();
        }
        void makeTableClass()
        {
                table.addElement(module.equipment.Name,module.mark);
                foreach (MEPSystem s in module.systems)
                {
                    foreach (Element e in s.Elements)
                    {
                        table.addElement(e.Name,"");
                    }
                }
            
            foreach (MEPSystem s in module.alertSystems)
            {
                foreach (Element e in s.Elements)
                {
                    table.addElement(e.Name, "");
                }
            }
        }
        public void CreateTable(Document doc)
        {
            makeTableClass();
            FamilySymbol famToPlace = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>().Where(x => x.Name == "TABLEHEADER").FirstOrDefault();
            Transaction trans = new Transaction(doc);
            trans.Start("Помещен на рисунок");
            FamilyInstance header = doc.Create.NewFamilyInstance(start, famToPlace,module.getVD());
            trans.Commit();
            XYZ point = new XYZ(start.X, start.Y - header.LookupParameter("Ширина").AsDouble() * 10, 0);
            foreach (TableElement elem in table.elements)
            {
                famToPlace = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>().Where(x => x.Name == "TABLESTRING").FirstOrDefault();
                trans.Start("Помещен на рисунок");
                FamilyInstance str = doc.Create.NewFamilyInstance(point, famToPlace, module.getVD());
                str.LookupParameter("Позиция").Set(elem.getPosition());
                str.LookupParameter("Колличество").Set(elem.getCount());
                str.LookupParameter("Наименование").Set(elem.getName());
                point = new XYZ(point.X, point.Y - str.LookupParameter("Ширина").AsDouble() * 10, 0);
                trans.Commit();
            }
        }

    }
}
