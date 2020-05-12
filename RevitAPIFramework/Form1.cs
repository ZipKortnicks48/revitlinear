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
    public partial class Form1 : Form
    {
        public List<ARKModule> blocks;
        private string[] files;
        private Func<List<ARKModule>,bool> action;
        public Form1(List<ARKModule> _blocks, Func<List<ARKModule>,bool> action)
        {
            this.action = action;
            blocks = _blocks;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            foreach (ARKModule module in blocks) {
                int rowNumber = dataGridView1.Rows.Add();
                dataGridView1.Rows[rowNumber].Cells["ColumnARK"].Value = module.getFullName();
            }
            string set_path = "settings.set";
            string folder_path = File.ReadAllText("settings.set");
            label2.Text = folder_path;
            files = Directory.GetFiles(folder_path, "*.rfa");
            label4.Text = files.Length.ToString();
            bindingSource1.DataSource = files;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //foreach(ARKModule block in blocks)
            //{
            //    block.files = this.files;
            //}
            //this.action(this.blocks);
            string path = "families.set";
            File.WriteAllText(path,"");
            List<string> families=new List<string>();
            for(int i=0;i<blocks.Count;i++)
            { families.Add(dataGridView1.Rows[i].Cells["ColumnFamily"].Value.ToString()); }
            File.AppendAllLines(path, families);
        }

        //сохранение папки семейств, обновление настроек
        private void button2_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    string path = "settings.set";
                        string createText = fbd.SelectedPath; 
                        File.WriteAllText(path, createText);                    
                }
            }
            MessageBox.Show("Путь к папке семейств изменен. Для того, чтобы изменения вступили в силу - перезаупустите плагин");
        }
    }
}
