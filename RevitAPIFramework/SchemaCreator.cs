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
        public List<string> staticFamilies = new List<string> { "ARKRIGHTOUTPUT.rfa", "BTH.rfa", "BTM.rfa", "ARKRIGHEMPTY.rfa","GAP.rfa", "TABLESTRING.rfa","TABLEHEADER.rfa" }; 
        void getBasEquipments(Document doc)
        {

            FilteredElementCollector systems
             = new FilteredElementCollector(doc)
               .OfClass(typeof(MEPSystem));
            IOrderedEnumerable<MEPSystem> systemsSorted =  from MEPSystem s in systems orderby Int32.Parse(s.Name) ascending select s;
            int i = 0;
            foreach (MEPSystem system in systemsSorted)
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
                b.setVD(vd);
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

            foreach (ARKModule b in ARKBLocks)
            {
                b.createTable(doc, new XYZ(10, 3, 0));
            }
            

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
            foreach (string s in staticFamilies)
            {
                string file_path = File.ReadAllText("settings.set");
                file_path += "\\static\\"+s;
                loader.LoadFamilyIntoProject(file_path, doc);
            }
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
                next = doc.Create.NewFamilyInstance(new XYZ(point.X, point.Y - index * len * 10, 0), famToPlace, view);
                trans.Commit();
                trans.Start("добавление параметров");
                next.LookupParameter("ark").Set(Int32.Parse(ark.mark.Remove(ark.mark.IndexOf("ARK"), 3)));
                next.LookupParameter("номер шлейфа").Set(Double.Parse(mep.Name));
                trans.Commit();
                DrawSensors(new XYZ(point.X + next.LookupParameter("Длина").AsDouble() * 10, point.Y - index * len * 10, 0), mep, Int32.Parse(ark.mark.Remove(ark.mark.IndexOf("ARK"), 3)), view, doc);
                len = next.LookupParameter("Ширина").AsDouble();
                 ++index;
            }
            if (ark.systems.Count <= ark.revitModule.Symbol.LookupParameter("Количество шлейфов справа").AsInteger())
            {
                DrawRemain(new XYZ(point.X, point.Y - (index * len * 10)+0.08, 0),doc,view, ark.revitModule.Symbol.LookupParameter("Количество шлейфов справа").AsInteger(),ark);
            }
            else
            {
                throw new Exception("Ошибка! Количество шлейфов в выбранном семействе меньше, чем в Revit-модели!");
            }
        }
        void DrawRemain(XYZ start, Document doc, ViewDrafting view, int last, ARKModule ark) {
            if (last - ark.systems.Count > 3)//изменить для разрыва
            {
                Transaction trans = new Transaction(doc);
                trans.Start("Помещен на рисунок");
                FamilySymbol famToPlace = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>().Where(x => x.Name == "GAP").FirstOrDefault();
                FamilySymbol emptyOutput = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>().Where(x => x.Name == "ARKRIGHEMPTY").FirstOrDefault();

                FamilyInstance gap = doc.Create.NewFamilyInstance(start, famToPlace, view);//разрыв
                double gapHeight = gap.Symbol.LookupParameter("Внутренняя высота").AsDouble();
                FilteredElementCollector collector = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol));
                List<AnnotationSymbolType> l = (from e in collector.ToElements() where e is AnnotationSymbolType select e as AnnotationSymbolType).ToList();
                double outputHeight = 1;
                foreach (AnnotationSymbolType at in l)
                {
                    if (at.FamilyName == "ARKRIGHEMPTY")
                    {
                        outputHeight = at.LookupParameter("Ширина").AsDouble();
                    }
                }

                start = new XYZ(start.X, start.Y - gapHeight * 10 - outputHeight * 10 + 0.08, 0);

                FamilyInstance next = doc.Create.NewFamilyInstance(start, emptyOutput, view);
                next.LookupParameter("номер шлейфа").Set(last - 4);
                next.LookupParameter("ark").Set(Int32.Parse(ark.mark.Remove(ark.mark.IndexOf("ARK"), 3)));
                for (int i = last - 3; i <= last; ++i)
                {
                    double height = next.Symbol.LookupParameter("Ширина").AsDouble();
                    start = new XYZ(start.X, start.Y - height * 10, 0);
                    next = doc.Create.NewFamilyInstance(start, emptyOutput, view);
                    next.LookupParameter("номер шлейфа").Set(i);
                    next.LookupParameter("ark").Set(Int32.Parse(ark.mark.Remove(ark.mark.IndexOf("ARK"), 3)));
                }
                trans.Commit();
            }
            else {
                FamilySymbol emptyOutput = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>().Where(x => x.Name == "ARKRIGHEMPTY").FirstOrDefault();
                start = new XYZ(start.X, start.Y - 0.08, 0);
                Transaction trans = new Transaction(doc);
                trans.Start("Помещен на рисунок");
                for (int i= ark.systems.Count+1;i<= last;i++) {  
                    FamilyInstance next = doc.Create.NewFamilyInstance(start, emptyOutput, view);
                    next.LookupParameter("номер шлейфа").Set(i);
                    next.LookupParameter("ark").Set(Int32.Parse(ark.mark.Remove(ark.mark.IndexOf("ARK"), 3))); 
                    double height = next.Symbol.LookupParameter("Ширина").AsDouble();
                    start = new XYZ(start.X, start.Y - height * 10, 0);
                }
                trans.Commit();
            }

        }
        void DrawSensors(XYZ point, MEPSystem mep, int ark, ViewDrafting view, Document doc)
        {
            int Dim = 0;
            int Hand= 0;
           
            foreach(Element e in mep.Elements)
            {
                if (e.Name.Contains("дым"))
                {
                    
                    ++Dim;
                }
                if (e.Name.Contains("ИПР"))
                {
                    ++Hand;
                }
            }
            if (Dim > 0)
            {
                if (Dim == 1) { point = drawOne(doc, view, true, 1,Double.Parse(mep.Name), ark, point); }
                if (Dim == 2) { point = drawTwo(doc, view, true, Double.Parse(mep.Name), ark, point); }
                if (Dim > 2) { point = drawMore(doc, view, true, Dim, Double.Parse(mep.Name), ark, point); }
            }
            if (Hand > 0) {
                if (Dim > 0)
                {
                    geometry.AddLines(doc,view,geometry.ConnectLinesByPoints(new List<XYZ> { point, new XYZ(point.X + 0.5, point.Y, 0) }));
                    point = new XYZ(point.X + 0.5, point.Y, 0);
                }
                //нарисовать соединение
                if (Hand == 1) { point = drawOne(doc, view, false, 1, Double.Parse(mep.Name), ark, point); }
                if (Hand == 2) { point = drawTwo(doc, view, false, Double.Parse(mep.Name), ark, point); }
                if (Hand >2) { point = drawMore(doc, view, false, Hand, Double.Parse(mep.Name), ark, point); }
            }

        }
        XYZ drawOne(Document doc,ViewDrafting view, bool type,int count,double shleif,int ark,XYZ start)
        {
            
            FamilySymbol famToPlace = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>().Where(x => type? x.Name == "BTH":x.Name=="BTM").FirstOrDefault();
            Transaction trans = new Transaction(doc);
            trans.Start("Помещен на рисунок");
            FamilyInstance sensor=doc.Create.NewFamilyInstance(start, famToPlace, view);
            trans.Commit();
            trans.Start("Помещен на рисунок");
            sensor.LookupParameter("ark").Set(ark);
            sensor.LookupParameter("нпп").Set(count);
            sensor.LookupParameter("шлейф").Set((Int32)shleif);
            trans.Commit();
            return new XYZ(start.X + sensor.Symbol.LookupParameter("Ширина").AsDouble() * 10,start.Y,0); 
        }
        XYZ drawTwo(Document doc, ViewDrafting view, bool type, double shleif, int ark, XYZ start)
        {
            XYZ lineStart = drawOne(doc,view,type,1,shleif,ark,start);
            XYZ lineEnd= new XYZ(lineStart.X+0.5,lineStart.Y,0);

            geometry.AddLines(doc, view, geometry.ConnectLinesByPoints(new List<XYZ> { lineStart,lineEnd}));
           return lineStart = drawOne(doc, view, type, 2, shleif, ark, lineEnd);

        }
        XYZ drawMore(Document doc, ViewDrafting view, bool type,int count,double shleif, int ark, XYZ start)
        {
            XYZ lineStart = drawOne(doc, view, type, 1, shleif, ark, start);
            XYZ lineEnd = new XYZ(lineStart.X + 0.5, lineStart.Y, 0);
            geometry.AddDottedLine(lineStart,lineEnd,doc,view);
            //geometry.AddLines(doc, view, geometry.ConnectLinesByPoints(new List<XYZ> { lineStart, lineEnd }));
            return lineStart = drawOne(doc, view, type , count, shleif, ark, lineEnd);
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
