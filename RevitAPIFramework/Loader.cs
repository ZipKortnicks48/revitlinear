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


namespace RevitAPIFramework
{
    class Loader
    {
         public void LoadFamilyIntoProject(string file_path, Document doc)
        {
            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Загрузка семейства");
                doc.LoadFamily(file_path);
                tx.Commit();
            }
        }
        public void DrawFamilyByCoordinate()
        {

        }
    }
}
