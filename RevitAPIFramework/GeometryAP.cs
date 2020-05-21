using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
namespace RevitAPIFramework
{
    class GeometryAP
    {
        public List<Line> ConnectLinesByPoints(List<XYZ> points)
        {
            List<Line> lines = new List<Line>();
            for(int i=0;i<points.Count-1;i++)
            {
               lines.Add(Line.CreateBound(points[i], points[i+1]));
            }
            return lines;
        }
        public void AddLines(Document doc,ViewDrafting view,List<Line>lines)
        {
            foreach (Line l in lines)
            {
                Transaction trans = new Transaction(doc);
                trans.Start("Отрисовка линии: " );
                doc.Create.NewDetailCurve(view, l);
                trans.Commit();
            }
        }
        public void AddDottedLine(XYZ start, XYZ end, Document doc,ViewDrafting vd)
        {
            Element style=null;
            ICollection<Element> elements = new FilteredElementCollector(doc).OfClass(typeof(GraphicsStyle)).ToElements();
            foreach (Element element in elements)
            {
                if (element.Name == "<Снесено>")
                    style = element;
            }
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Преобразование линий");
                DetailCurve l = doc.Create.NewDetailCurve(vd, Line.CreateBound(start, end));
                l.LineStyle = style ;
                t.Commit();
            }    
          }       
    }
}
