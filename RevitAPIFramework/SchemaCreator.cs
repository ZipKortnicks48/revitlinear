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
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    class SchemaCreator : IExternalCommand
    {
        public List<RevitAPIFramework.ARKModule> ARKBLocks = new List<RevitAPIFramework.ARKModule>();//набор блоков, которые отрисовывать
        public RevitAPIFramework.Form1 setForm;//форма настроек
        public string[] pathes;
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
                       
           
        }
        private void getPathes()
        {
            this.pathes = File.ReadAllLines("families.set");
            int i = 0;
            foreach (ARKModule block in ARKBLocks) {
                string pathes2 = pathes[i].Replace('\\','\'');
                string[] strings = pathes2.Split('\'');
                block.addFilepath(pathes[i]);
                block.addFilename(strings[strings.Length - 1].Replace(".rfa",""));
                i++;
            }
        }
        public void loadFamilies(Document doc)
        {

            foreach (ARKModule module in ARKBLocks)
            {
                FilteredElementCollector a = new FilteredElementCollector(doc).OfClass(typeof(Family));
                Family family = a.FirstOrDefault<Element>(e => e.Name.Equals(module.filename)) as Family;
                if (null == family)
                {
                    string FamilyPath = module.filepath;

                    // Загрузка семейства из файла.
                    using (Transaction tx = new Transaction(doc))
                    {
                        tx.Start("Загрузка семейства");
                        doc.LoadFamily(FamilyPath, out family);
                        tx.Commit();
                    }
                }
               // FilteredElementCollector a = new FilteredElementCollector(doc).OfClass(typeof(Family));
            }
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
            FilteredElementCollector collector = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol));
            //List<AnnotationSymbolType> l = new List<AnnotationSymbolType>();
            //l = (from e in collector.ToElements() where e is AnnotationSymbolType select e as AnnotationSymbolType).ToList();
            /*if (l.Any(e=>e.FamilyName=="ark-module-1"))
                { 
                Console.WriteLine("good");
                }*/
            foreach (ARKModule b in ARKBLocks)
            {
                FamilySymbol famToPlace=null;
                ViewFamilyType vt = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>().FirstOrDefault(vft => vft.ViewFamily == ViewFamily.Drafting);
                ElementId id = vt.Id;
                Transaction trans = new Transaction(doc);
                trans.Start("Отрисовка");
                ViewDrafting vd = ViewDrafting.Create(doc, id);
                ElementId viewId = vd.Id;
                foreach(FamilySymbol f in collector)
                {
                    if (f.Name == b.filename)
                    {
                        famToPlace = f;
                        foreach (Parameter p in famToPlace.Parameters)
                        {
                            if (p.Definition.Name == "ark-module")
                            {
                                p.Set(b.id.ToString());
                            }
                        }
                        break;
                    }
                }
               
                doc.Create.NewFamilyInstance(new XYZ(0,0,0),famToPlace,vd);
                trans.Commit();
            }
        }
        void DrawOne(Document doc)
        {

        }
        public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements)
        {
            //Получение объектов приложения и документа
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;
            this.getBasEquipments(doc);//собираем в класс
            this.getPathes();
            this.loadFamilies(doc);
            this.DrawAll(doc);//отрисовка
            return Result.Succeeded;
        }
    }
}
