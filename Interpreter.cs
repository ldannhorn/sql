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

            string[] args = query.Split(' ');

            switch (args[0].ToLower())
            {
                case "insert":
                    Insert(args);
                    break;
            }


            return "";
        }

        private bool Insert(string[] query)
        {
            return false;
        }


    }
}
