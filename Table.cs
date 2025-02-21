using System;
using System.Collections.Generic;
using System.Linq;


namespace ProjektSQL
{
    public class Table
    {
        public string name;
        private string[] attributes;
        private List<Record> records;

        // Konstruktor 1 mit Name und Attributen, Leere Datensätze werden erstellt
        public Table(string name, string[] attributes)
        {
            this.name = name.ToLower();
            this.attributes = attributes;
            records = new List<Record>();
        }

        // Konstruktor 2 mit Name, Attributen, IDs und Datensätzen
        public Table(string name, string[] attributes, List<int> ids, List<string[]> table)
        {
            this.name = name.ToLower();
            this.attributes = attributes;
            
            if (ids.Count == table.Count)
            {
                records = new List<Record>();
                for (int i = 0; i < ids.Count; i++)
                {
                    records.Add(new Record(ids[i], table[i]));
                }
            }
            else
            {
                records = new List<Record>();
            }
        }

        // Konstruktor 3 mit Name, Attributen und Datensätzen
        public Table(string name, string[] attributes, List<Record> records)
        {
            this.name = name.ToLower();
            this.attributes = attributes;
            this.records = records;
        }


        public bool Insert(int id, string[] values)
        // Fügt einen Datensatz in die Tabelle ein sofern die ID noch nicht existiert
        {
            int[] ids = GetIDs();
            if (ids.Contains(id)) return false;

            records.Add(new Record(id, values));
            return true;
        }

        public bool Insert(string[] id_values)
        // Fügt einen Datensatz in die Tabelle ein sofern die ID noch nicht existiert
        // Die ID wird dem ersten Wert des Arrays entnommen
        {
            int id;
            bool idIsInt = int.TryParse(id_values[0], out id);
            if (!idIsInt) return false;

            string[] values = new string[id_values.Length - 1];
            Array.Copy(id_values, 1, values, 0, id_values.Length - 1);

            records.Add(new Record(id, values));
            return true;
        }

        public bool Update(int id, string[] values)
        // Aktualisiert einen Datensatz in der Tabelle sofern die ID existiert
        {
            int[] ids = GetIDs();
            if (!ids.Contains(id)) return false;
            Select(id).setValues(values);
            return true;
        }

        public bool Delete(int id)
        // Löscht einen Datensatz in der Tabelle sofern die ID existiert
        {
            int[] ids = GetIDs();
            if (!ids.Contains(id)) return false;
            records.Remove(Select(id));
            return true;
        }

        public Record Select(int id)
        // Gibt einen Datensatz in der Tabelle zurück sofern die ID existiert, sonst leer
        {
            int[] ids = GetIDs();
            if (!ids.Contains(id)) return new Record(-1, new string[attributes.Length]);
            return records[Array.IndexOf(ids, id)];
        }

        public int IndexOfAttribute(string attribute)
        // Gibt den Index eines Attributs zurück, -1 wenn nicht vorhanden
        {
            for (int i=0; i<attributes.Length; i++)
            {
                if (attributes[i] == attribute) return i;
            }
            return -1;
        }

        public int[] GetIDs()
        // Gibt alle IDs der Tabelle zurück
        {
            int[] ids = new int[records.Count];
            for (int i = 0; i < records.Count; i++)
            {
                ids[i] = records[i].getId();
            }

            return ids;
        }

        public Table Copy()
        {
            string[] newAttributes = new string[attributes.Length];
            attributes.CopyTo(newAttributes, 0);

            List<Record> newRecords = new List<Record>();
            foreach (var record in records)
            {
                newRecords.Add(new Record(record.getId(), record.getValues()));
            }

            return new Table(name, newAttributes, newRecords);
        }
    }
}
