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
        public GeometryAP geometry=new GeometryAP();
        public List<ElementId> drawingviews = new List<ElementId>();
        public List<ElementId> arkmoduleIds = new List<ElementId>();
        public Loader loader=new Loader();

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
                    m.mark = GetMark(system.BaseEquipment);
                    ARKBLocks.Add(m);
                }
                findAndAddSystem(system);
            }
                       
           
        }
        string GetMark(FamilyInstance ark)
        {
            string mark = ark.LookupParameter("Марка").AsString();
            return mark;
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
                    /*using (Transaction tx = new Transaction(doc))
                    {
                        tx.Start("Загрузка семейства");
                        doc.LoadFamily(FamilyPath, out family);
                        tx.Commit();
                    }*/
                    loader.LoadFamilyIntoProject(FamilyPath,doc);
                }
            }
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
            foreach (ARKModule b in ARKBLocks)
            {
                FamilySymbol famToPlace=null;
                ViewFamilyType vt = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>().FirstOrDefault(vft => vft.ViewFamily == ViewFamily.Drafting);
                ElementId id = vt.Id;
                Transaction trans = new Transaction(doc);
                trans.Start("Отрисовка");
                ViewDrafting vd = ViewDrafting.Create(doc, id);
                ElementId viewId = vd.Id;
                drawingviews.Add(viewId);
                famToPlace = collector.Cast<FamilySymbol>().Where(x => x.Name == b.filename).First();
                b.revitSymbol = famToPlace;               
                b.revitModule=doc.Create.NewFamilyInstance(new XYZ(0,0,0),famToPlace,vd);
                arkmoduleIds.Add(b.revitModule.Id);
                trans.Commit();
                
            }

            SetArkIndexes(doc);

            DrawLines(doc);

            
            

        }
        void SetArkIndexes(Document doc)
        {
            FilteredElementCollector placedCollector = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance));
            foreach (ARKModule b in ARKBLocks)
            {
                foreach (FamilyInstance f in placedCollector)
                {
                    
                    if (f.Id.ToString() == b.revitModule.Id.ToString())
                    {
                        Parameter param=f.LookupParameter("ark-module");
                        
                        Parameter param2 = f.LookupParameter("ark-module");
                        using (Transaction t = new Transaction(doc, "Добавление индексов"))
                        {
                            t.Start();
                            param2.Set(Int32.Parse(b.mark.Remove(b.mark.IndexOf("ARK"), 3)));
                            t.Commit();
                        }
                        break;
                    }
                }
            }
        }
        void DrawLines(Document doc)
        {
            
            foreach (ElementId vd in drawingviews)
            {
                
                ViewDrafting view = new FilteredElementCollector(doc).OfClass(typeof(ViewDrafting)).Cast<ViewDrafting>().Where(x => x.Id==vd).FirstOrDefault();
                FamilyInstance ark  = new FilteredElementCollector(doc, vd).OfClass(typeof(FamilyInstance)).Cast<FamilyInstance>().Where(x=>arkmoduleIds.Contains(x.Id)).FirstOrDefault();
                List<XYZ> points = new List<XYZ>();
                points.Add(new XYZ(ark.Symbol.LookupParameter("Ширина").AsDouble() * 10 / 2 , ark.Symbol.LookupParameter("Высота").AsDouble() * 10 / 2, 0));
                points.Add(new XYZ(ark.Symbol.LookupParameter("Ширина").AsDouble() * 10 / 2, ark.Symbol.LookupParameter("Высота").AsDouble() * 10 / 2 + 0.5, 0));
                points.Add(new XYZ(ark.Symbol.LookupParameter("Ширина").AsDouble() * 10 / 2 + 2, ark.Symbol.LookupParameter("Высота").AsDouble() * 10 / 2 + 0.5, 0));
                geometry.AddLines(doc,view,geometry.ConnectLinesByPoints(points));
                DrawShleifs(points[2],doc,view);
            }
            
        }
        
        void DrawShleifs(XYZ point, Document doc, ViewDrafting view)
        {
            //добавление семейтсва
            string file_path=File.ReadAllText("settings.set");
            file_path += "\\static\\ARKRIGHTOUTPUT.rfa";
            loader.LoadFamilyIntoProject(file_path,doc);
            //соответствие арк
            ARKModule ark = null;
            FamilyInstance f = new FilteredElementCollector(doc,view.Id).OfClass(typeof(FamilyInstance)).Cast<FamilyInstance>().Where(x => arkmoduleIds.Contains(x.Id)).FirstOrDefault();
            foreach (ARKModule module in ARKBLocks)
            {
                if (module.revitModule.Id == f.Id)
                {
                    ark = module;
                }
            }
            //добавление экземпляров на виды по точке
                double len = 0;
                int index = 0;
                FamilyInstance next = null;
                foreach (MEPSystem mep in ark.systems)
                {
                    FamilySymbol famToPlace = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>().Where(x => x.Name == "ARKRIGHTOUTPUT").FirstOrDefault();
                Transaction trans = new Transaction(doc);
                trans.Start("Помещен на рисунок");
                    next=doc.Create.NewFamilyInstance(new XYZ(point.X, point.Y-index*len*10, 0), famToPlace, view);
                trans.Commit();
                trans.Start("добавление параметров");
                    next.LookupParameter("ark").Set(Int32.Parse(ark.mark.Remove(ark.mark.IndexOf("ARK"), 3)));
                trans.Commit();
                trans.Start("добавление параметров");
                next.LookupParameter("номер шлейфа").Set(Double.Parse(mep.Name));
                trans.Commit();
                len = next.LookupParameter("Ширина").AsDouble();
               
                ++index;

            }
            
            //расстановка параметров
            //добавлегте 

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
