using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RevitAPIFramework
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        public List<ARKModule> blocks; 
        private string[] files;
        private Func<List<ARKModule>,bool> action;
        private SettingSectionsCreator settings;
        public Form1(List<ARKModule> _blocks, Func<List<ARKModule>,bool> action)
        {
            this.action = action;
            blocks = _blocks;
            settings = new SettingSectionsCreator();
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                string[] s = File.ReadAllLines("C://ProgramData//Autodesk//Revit//Addins//2019//Linear//intocabin.set");
                textBox1.Text = s[0];
                textBox2.Text = s[1];
                textBox5.Text = s[2];
            }
            catch (Exception ex){
                MessageBox.Show(ex.ToString());
            }
            try
            {
                foreach (ARKModule module in blocks)
                {
                    int rowNumber = dataGridView1.Rows.Add();
                    dataGridView1.Rows[rowNumber].Cells["ColumnARK"].Value = module.getFullName();
                }
                string set_path = "C://ProgramData//Autodesk//Revit//Addins//2019//Linear//settings.set";
                string folder_path = File.ReadAllText("C://ProgramData//Autodesk//Revit//Addins//2019//Linear//settings.set");
                label5.Text = folder_path;
                files = Directory.GetFiles(folder_path, "*.rfa");
                if (files.Length != 0)
                {
                    label6.Text = files.Length.ToString();
                    bindingSource1.DataSource = files;
                    set_path = "C://ProgramData//Autodesk//Revit//Addins//2019//Linear//families.set";
                    string[] strs = File.ReadAllLines(set_path);
                    int last;
                    if (strs.Length > blocks.Count) last = blocks.Count; else last = strs.Length;
                    for (int i = 0; i < last; i++)
                    {
                        dataGridView1.Rows[i].Cells["ColumnFamily"].Value = strs[i];
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            try {
                foreach (ARKModule m in blocks)
                {
                    listBox1.Items.Add(m.revitModule.LookupParameter("Марка").AsString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            try {
                settings.loadSettings();
            } catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string path = "C://ProgramData//Autodesk//Revit//Addins//2019//Linear//families.set";
                File.WriteAllText(path, "");
                List<string> families = new List<string>();
                for (int i = 0; i < blocks.Count; i++)
                { families.Add(dataGridView1.Rows[i].Cells["ColumnFamily"].Value.ToString()); }
                File.AppendAllLines(path, families);
                MessageBox.Show("Настройки сохранены.");
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
        }

        //сохранение папки семейств, обновление настроек
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    DialogResult result = fbd.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        string path = "C://ProgramData//Autodesk//Revit//Addins//2019//Linear//settings.set";
                        string createText = fbd.SelectedPath;
                        File.WriteAllText(path, createText);
                    }
                }
                MessageBox.Show("Путь к папке семейств изменен. Для того, чтобы изменения вступили в силу - перезаупустите плагин");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            try {
                string[] str = new string[3];
                str[0] = textBox1.Text;
                str[1] = textBox2.Text;
                str[2] = textBox5.Text;
                File.WriteAllLines("C://ProgramData//Autodesk//Revit//Addins//2019//Linear//intocabin.set", str);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
        }

        private void getAmpForce() { 
            
        }
        private double getNormalCount(double c)
        {
            try
            {
                int count = Convert.ToInt32(c);
                if (count % 5 == 0)
                { return Convert.ToDouble(count); }
                else { count += 5 - (count % 5); return Convert.ToDouble(count); }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
                return 0;
            }
        }
        private double getNormalS(double c)
        {
            return 0;
        }
        private double getI(MEPSystem mep) {
            int countsensors = 0;
            double I = 0;
            foreach (FamilyInstance elem in mep.Elements)
            {
                if (elem.Symbol.LookupParameter("Потребляемый ток") != null)
                    I += elem.Symbol.LookupParameter("Потребляемый ток").AsDouble();
                countsensors++;
            }
            I = I / countsensors;
            return I;

        }
        private double getS(double i,double L)
        {
            return ((i/1000)*L*(2*0.0175)/6);
        }
        private double getL(MEPSystem m,int L)
        {
            return m.LookupParameter("Длина").AsDouble() / L;
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                comboBox1.Items.Clear();
                int selectedind = listBox1.SelectedIndex;
                ARKModule ark = blocks[selectedind];
                MEPSystem mep = null;
                foreach (MEPSystem m in ark.systems)
                {
                    if (m.Name.Contains("1")) { mep = m; }
                }
                double len = getL(mep, ark.systems.Count);
                len = getNormalCount(len);
                double I = 0;
                I = getI(mep);
                label23.Text = len.ToString();
                label24.Text = I.ToString();

                foreach (MEPSystem m in ark.systems)
                {
                    string name = m.Name + "-й шлейф";
                    double s = getS(getI(mep), getL(m, ark.systems.Count));
                    comboBox1.Items.Add(name + ": " + s.ToString());
                }
                try
                {
                    int set_index = settings.loadSettingByARK(listBox1.SelectedItem.ToString());
                    SettingSections set = settings.getByIndex(set_index);
                    string[] sections = set.section.Split('x');
                    textBox4.Text = set.mark;
                    textBox6.Text = sections[0];
                    textBox7.Text = sections[1];
                    textBox3.Text = sections[2];
                }
                catch
                {

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
              
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            settings.writeToFile();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                int index = settings.loadSettingByARK(listBox1.SelectedItem.ToString());
                if (index < settings.Count()) { settings.getByIndex(index).SettingSectionsFromForm(listBox1.SelectedItem.ToString(), textBox4.Text, textBox6.Text, textBox7.Text, textBox3.Text); }
                else
                {
                    SettingSections s = new SettingSections();
                    s.SettingSectionsFromForm(listBox1.SelectedItem.ToString(), textBox4.Text, textBox6.Text, textBox7.Text, textBox3.Text);
                    settings.Add(s);
               }
                List<string> namesARK = new List<string>();
                foreach (Object str in listBox1.Items)
                {
                    namesARK.Add(str.ToString());
                }
                settings.deleteIfNot(namesARK);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
               
            }
        }
    }
}
