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
using System.Linq.Expressions;
using System.Windows.Forms;

namespace RevitAPIFramework
{

    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class starter : IExternalCommand
    {

        public List<RevitAPIFramework.ARKModule> ARKBLocks = new List<RevitAPIFramework.ARKModule>();//набор блоков, которые отрисовывать
        public RevitAPIFramework.Form1 setForm;//форма настроек
        static string PluralSuffix(int n)
        {
            return 1 == n ? "" : "s";

        }
        void getBasEquipments(Document doc)
        {

            FilteredElementCollector systems
             = new FilteredElementCollector(doc)
               .OfClass(typeof(MEPSystem));
            int i = 0;
            foreach (MEPSystem system in systems)
            {
                string sysId = system.BaseEquipment.Id.ToString();
                if (!this.ARKBLocks.Any(element => element.id == sysId))
                {
                   
                        ARKModule m = new ARKModule(sysId, i, system.BaseEquipment);
                        ARKBLocks.Add(m);
                    
                }
                
                findAndAddSystem(system);

            }
            setForm = new RevitAPIFramework.Form1(ARKBLocks, SaveEveryARKBlockFamily);
            setForm.Show();
        }
        public bool SaveEveryARKBlockFamily(List<ARKModule> am)
        {
            this.ARKBLocks = am;
            return true;
        }

        public delegate void SaveAB(List<ARKModule> module);
        //Добавление системы шлейфа к блоку
        void findAndAddSystem(MEPSystem system)
        {
            ARKBLocks.ForEach(e => { if (e.id == system.BaseEquipment.Id.ToString()) { e.addSystem(system); } });
        }
        void DrawAll(Document doc)
        {
            foreach (ARKModule b in ARKBLocks)
            {
                ViewFamilyType vt = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>().FirstOrDefault(vft => vft.ViewFamily == ViewFamily.Drafting);
                ElementId id = vt.Id;
                Transaction trans = new Transaction(doc);
                trans.Start("Lab");
                ViewDrafting vd = ViewDrafting.Create(doc, id);
                trans.Commit();
            }
        }
        void DrawOne(Document doc)
        {

        }

        void TraverseSystems(Document doc)
        {
            FilteredElementCollector systems
              = new FilteredElementCollector(doc)
                .OfClass(typeof(MEPSystem));

            int i, n;
            string s;
            string[] a;

            StringBuilder message = new StringBuilder();

            foreach (MEPSystem system in systems)
            {
                message.AppendLine("System Name: "
                  + system.Name);

                message.AppendLine("Base Equipment: "
                  + system.BaseEquipment.Name);

                ConnectorSet cs = system.ConnectorManager
                  .Connectors;

                i = 0;
                n = cs.Size;
                a = new string[n];

                s = string.Format(
                  "{0} element{1} in ConnectorManager: ",
                  n, PluralSuffix(n));

                foreach (Connector c in cs)
                {
                    Element e = c.Owner;

                    if (null != e)
                    {
                        a[i++] = e.GetType().Name
                          + " " + e.Id.ToString();
                    }
                }

                message.AppendLine(s
                  + string.Join(", ", a));

                i = 0;
                n = system.Elements.Size;
                a = new string[n];

                s = string.Format(
                  "{0} element{1} in System: ",
                  n, PluralSuffix(n));

                foreach (Element e in system.Elements)
                {
                    a[i++] = e.GetType().Name
                      + " " + e.Id.ToString();
                }

                message.AppendLine(s
                  + string.Join(", ", a));
            }

            n = systems.Count<Element>();

            string caption =
              string.Format("Traverse {0} MEP System{1}",
              n, (1 == n ? "" : "s"));

            TaskDialog dlg = new TaskDialog(caption);
            dlg.MainContent = message.ToString();
            dlg.Show();

        }
        public Result Execute(
        ExternalCommandData commandData,
        ref string message,
        ElementSet elements)
        {
            try
            {
                //Получение объектов приложения и документа
                UIApplication uiApp = commandData.Application;
                Document doc = uiApp.ActiveUIDocument.Document;
                this.getBasEquipments(doc);
                //TraverseSystems(doc);
                FilteredElementCollector collector = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol));
                List<AnnotationSymbolType> l = new List<AnnotationSymbolType>();
                l = (from e in collector.ToElements() where e is AnnotationSymbolType select e as AnnotationSymbolType).ToList();
                /*if (l.Any(e=>e.FamilyName=="ark-module-1"))
                    { 
                    Console.WriteLine("good");
                    }*/
                foreach (AnnotationSymbolType at in l)
                {
                    if (at.FamilyName == "ark-module-1")
                    {
                        Family f = at.Family;
                        Document famDoc = doc.EditFamily(at.Family);

                        //TaskDialog.Show("Family PathName", famDoc.PathName);
                        Console.WriteLine("good");
                    }
                }
                return Result.Succeeded;
            }
            catch (Exception e){ MessageBox.Show(e.Message); return Result.Succeeded; }
        } 
    }
}