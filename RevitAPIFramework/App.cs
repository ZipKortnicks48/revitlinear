using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Electrical;
using RevitAPIFramework;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Security.Principal;
using System.Windows.Forms;

namespace RevitAPIFramework
{
    class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {

            // Method to add Tab and Panel 
            try
            {
                RibbonPanel panel = ribbonPanel(a);
                // Reflection to look for this assembly path 
                string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
                // Add button to panel
                PushButton button = panel.AddItem(new PushButtonData("Button", "Настройки", thisAssemblyPath, "RevitAPIFramework.starter")) as PushButton;
                // Add tool tip 
                button.ToolTip = "Задайте настройки семейств и рабочей папки.";
                // Reflection of path to image 
                var globePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "settings.jpg");
                Uri uriImage = new Uri(globePath);
                // Apply image to bitmap
                BitmapImage largeImage = new BitmapImage(uriImage);
                // Apply image to button 
                button.LargeImage = largeImage;

                // Add button to panel
                PushButton button1 = panel.AddItem(new PushButtonData("Button1", "Построить схемы", thisAssemblyPath, "RevitAPIFramework.SchemaCreator")) as PushButton;
                // Add tool tip 
                button1.ToolTip = "Запуск построения схем.";
                // Reflection of path to image 
                var globePath1 = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "play.jpg");
                Uri uriImage1 = new Uri(globePath1);
                // Apply image to bitmap
                BitmapImage largeImage1 = new BitmapImage(uriImage1);
                // Apply image to button 
                button1.LargeImage = largeImage1;

                a.ApplicationClosing += a_ApplicationClosing;

                //Set Application to Idling
                a.Idling += a_Idling;
            }
            catch (Exception e) {
                MessageBox.Show(e.ToString());
            }
            return Result.Succeeded;
        }

        //*****************************a_Idling()*****************************
        void a_Idling(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs e)
        {

        }

        //*****************************a_ApplicationClosing()*****************************
        void a_ApplicationClosing(object sender, Autodesk.Revit.UI.Events.ApplicationClosingEventArgs e)
        {
            throw new NotImplementedException();
        }

        //*****************************ribbonPanel()*****************************
        public RibbonPanel ribbonPanel(UIControlledApplication a)
        {
            string tab = "Построение линейной схемы"; // Tab name
            // Empty ribbon panel 
            RibbonPanel ribbonPanel = null;
            // Try to create ribbon tab. 
            try
            {
                a.CreateRibbonTab(tab);
            }
            catch { }
            // Try to create ribbon panel.
            try
            {
                RibbonPanel panel = a.CreateRibbonPanel(tab, "Построение линейных схем");
            }
            catch { }
            // Search existing tab for your panel.
            List<RibbonPanel> panels = a.GetRibbonPanels(tab);
            foreach (RibbonPanel p in panels)
            {
                if (p.Name == "Построение линейных схем")
                {
                    ribbonPanel = p;
                }
            }
            //return panel 
            return ribbonPanel;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}

