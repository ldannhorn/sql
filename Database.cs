using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektSQL
{
    internal class Database
    {
        private Table[] tables;

        public Database(Table[] tables)
        {
            this.tables = tables;
        }


        public Table GetTable(string table_name)
        {
            foreach (Table table in tables)
            {
                if (table.name == table_name)
                    return table.Copy();
            }
            return null;
        }
        public Table GetTable(int index)
        {
            Table table = tables[index].Copy();
            return table;
        }


        public Database Copy()
        {
            Table[] newTables = new Table[tables.Length];
            for (int i = 0; i < tables.Length; i++)
                newTables[i] = tables[i].Copy();
            return new Database(newTables);
        }

        public int GetTableAmount()
        {
            return this.tables.Length;
        }

        public void SetTable(int index, Table table)
        {
            tables[index] = table;
        }

    }
}
