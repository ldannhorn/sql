using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektSQL
{
    // Datensatz-Klasse
    public class Record
    {
        // Werte und erzwungener Primärschlüssel
        private int id;
        private string[] values;

        public Record()
        {
            id = -1;
            values = new string[0];
        }

        public Record(int id, string[] values)
        {
            this.id = id;
            this.values = values;
        }

        public int GetId()
        {
            return id;
        }

        public string GetValue(int index)
        {
            if (index < 0 || index >= values.Length) return null;
            return values[index];
        }

        public string[] GetValues()
        {
            string[] retValues = new string[values.Length];
            values.CopyTo(retValues, 0);
            return retValues;
        }

        public bool SetValues(string[] values)
        {
            if (values.Length != this.values.Length) return false;
            this.values = values;
            return true;
        }

        public bool Equals(Record record)
        {
            if (values.Length != record.GetValues().Length) return false;

            for (int i=0; i< values.Length; i++)
            {
                if (this.values[i] != record.GetValue(i))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
