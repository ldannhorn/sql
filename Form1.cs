using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjektSQL
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void textBoxSqlCommand_TextChanged(object sender, EventArgs e)
        {

        }
    }

    public class Table
    {
        private List<int> ids;
        private List<string[]> table;
        private string[] attributes;

        public Table(string[] attributes)
        {
            ids= new List<int>();
            table = new List<string[]>();
            this.attributes = attributes;
        }

        public Table(string[] attributes, List<int> id, List<string[]> table)
        {
            this.attributes = attributes;
            ids = id;
            this.table = table;
        }

        public bool Insert(int id, string[] values)
        {
            if (ids.Contains(id)) return false;
            ids.Add(id);
            table.Add(values);
            return true;
        }

        public bool Update(int id, string[] values)
        {
            if (!ids.Contains(id)) return false;
            table[ids.IndexOf(id)] = values;
            return true;
        }

        public bool Delete(int id)
        {
            if (!ids.Contains(id)) return false;
            int index = ids.IndexOf(id);
            ids.RemoveAt(index);
            table.RemoveAt(index);
            return true;
        }

        public string[] select(int id)
        {
            if (!ids.Contains(id)) return new string[attributes.Length];
            return table[ids.IndexOf(id)];
        }
    }
}
