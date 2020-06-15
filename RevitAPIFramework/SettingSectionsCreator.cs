using Autodesk.Revit.DB.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPIFramework
{
    class SettingSectionsCreator
    {
        List<SettingSections> settings;

        public SettingSectionsCreator() {  settings = new List<SettingSections>(); }
        public void loadSettings() {
            string[] str = File.ReadAllLines("C://ProgramData//Autodesk//Revit//Addins//2019//Linear//sections.set");
            foreach(string s in str)
            {
                string[] arrSet= s.Split('&');
                string ark = arrSet[0];
                arrSet = arrSet[1].Split(';');
                SettingSections set = new SettingSections();
                set.SettingSectionsFromFile(ark,arrSet[0],arrSet[1]);
                settings.Add(set);
            }
        }
        public void deleteIfNot(List<string> names) {
            foreach (SettingSections set in settings) {
                if (!names.Contains(set.ark)) {
                    settings.Remove(set);
                }
            }
        }
        public void writeToFile() {
            string[] result=new string[settings.Count];
            int index = 0;
            foreach(SettingSections s in settings)
            {
                result[index] = s.GetStringForFile();
                index++;
            }
            File.WriteAllLines("C://ProgramData//Autodesk//Revit//Addins//2019//Linear//sections.set", result);
        } 
        public int Count() { return settings.Count(); }
        public void Add(SettingSections s) { settings.Add(s); }
        public SettingSections getByIndex(int index)
        {
            return settings[index];
        }
        public int loadSettingByARK(string _ark) {
            int index = 0;
            foreach (SettingSections s in settings)
            {
                if (s.ark == _ark) {
                    return index;
                }
                index++;
            }
            return index;
        }

    }
}
