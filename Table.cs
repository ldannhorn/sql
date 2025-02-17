using System.Collections.Generic;


namespace ProjektSQL
{
    public class Table
    {
        public string name;
        private List<int> ids;
        private List<string[]> table;
        private string[] attributes;

        public Table(string name, string[] attributes)
        {
            this.name = name.toLower();
            ids = new List<int>();
            table = new List<string[]>();
            this.attributes = attributes;
        }

        public Table(string name, string[] attributes, List<int> id, List<string[]> table)
        {
            this.name = name.toLower();
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
        public bool Insert(string[] id_values)
        {
            int id;
            bool idIsInt = int.TryParse(id_values[0], out id);
            if (!idIsInt) return false;

            string[] values = new string[id_values.Length - 1];
            Array.Copy(query, 1, values, 0, id_values.Length - 1);

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
