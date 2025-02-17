using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektSQL
{
    class Interpreter
    {
        
        private Table[] tables;

        public Interpreter(Table[] tables)
        {
            this.tables = tables;
        }

        public string Interpret(string query)
        {

            string[] args = query.ToLower().Split(' ');

            switch (args[0])
            {
                case "insert":
                    Insert(args);
                    break;
                case "update":
                    Update(args);
                    break;
            }


            return "";
        }

        private bool Insert(string[] query)
        {
            /*
            INSERT Syntax:
            INSERT INTO table_name VALUES ("val1", "val2", "val3");
            */


            // 2. Argument INTO?
            if (query[1] != "into")
                return false;

            // Finde table (3. Argument)
            Table table = null;
            foreach (Table table1 in this.tables) {
                if (table1.name == query[2])
                {
                    table = table1;
                    break;
                }
            }
            // Name nicht vorhanden
            if (table == null)
                return false;

            // 4. Argument VALUES?
            if (query[3] != "values")
                return false;

            // Daten herausarbeiten
            string[] datapart = new string[query.Length - 4];
            Array.Copy(query, 4, datapart, 0, query.Length - 4);
            string data = string.join(' ', datapart); // Entfernte Leerzeichen wieder hinzufügen

            if (!data.EndsWith(");") || !data.StartsWith("("))
                return false;

            data = data.TrimStart("(\"").TrimEnd("\");");
            
            string[] args = data.Split("\", \"");

            return table.Insert(args);
        }

        private bool Update(string[] query)
        {
            /*
            UPDATE Symtax:
            UPDATE table_name SET ("value", "value") WHERE condition;
            */

            return false;
        }

    }
}
