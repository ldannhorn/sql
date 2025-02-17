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
            }


            return "";
        }

        private bool Insert(string[] query)
        {
            // 2. Argument INTO?
            if (query[1] != "into")
                return false;

            foreach (Table table in this.tables) {
                if (table.name)
            }

            return false;
        }


    }
}
