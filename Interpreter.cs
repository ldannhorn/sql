using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProjektSQL
{
    class Interpreter
    {
        
        private Database database;

        public Interpreter(Database database)
        {
            this.database = database;
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
            Table table = database.GetTable(query[2]);
            // Name nicht vorhanden
            if (table == null)
                return false;

            // 4. Argument VALUES?
            if (query[3] != "values")
                return false;

            // Daten herausarbeiten
            string[] datapart = new string[query.Length - 4];
            Array.Copy(query, 4, datapart, 0, query.Length - 4);
            string data = string.Join(" ", datapart); // Entfernte Leerzeichen wieder hinzufügen

            if (!data.EndsWith(");") || !data.StartsWith("("))
                return false;

            data = data.TrimStart('(', '"').TrimEnd('"', ')', ';');

            string[] args = Regex.Split(data, "\", \"");

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

        private Database Where(string conditions)
        {
            /*
            WHERE Syntax:
            WHERE condition AND condition OR condition

            Condition Syntax:
            table_name.attribute = "string"/table_name.attribute
            table_name.attribute != "string"/table_name.attribute
            table_name.attribute > "string"/table_name.attribute
            table_name.attribute < "string"/table_name.attribute
            table_name.attribute >= "string"/table_name.attribute
            table_name.attribute <= "string"/table_name.attribute
            */

            string[] arr_conditions = conditions.Split(new string[] { " and ", " or " }, StringSplitOptions.None);

            Database[] result = new Database[arr_conditions.Length];
            for (int i = 0; i < arr_conditions.Length; i++)
            {
                result[i] = Condition(arr_conditions[i]);
            }


        }


        private Database Condition(string condition)
        {
            /*
            Condition Syntax:
            table_name.attribute = "string"/table_name.attribute
            table_name.attribute != "string"/table_name.attribute
            table_name.attribute > "string"/table_name.attribute
            table_name.attribute < "string"/table_name.attribute
            table_name.attribute >= "string"/table_name.attribute
            table_name.attribute <= "string"/table_name.attribute
            */

            string[] args = condition.Split(' ');

            string tableName = args[0].Split('.')[0];
            string attribute = args[0].Split('.')[1];
            string op = args[1];

            // Rechten Teil wieder zusammensetzen
            string[] arr_comp = new string[args.Length - 2];
            Array.Copy(args, 2, arr_comp, 0, args.Length - 2);

            string comp = string.Join(" ", arr_comp);

            // Vergleich mit String oder anderem Attribut unterscheiden
            bool isString = comp.StartsWith("\"") && comp.EndsWith("\"");

            // Vergleiche durchführen
            Database result;
            switch (op)
            {
                case "=":
                    if (isString)
                    {
                        // Tabelle der linken Seite holen
                        Table leftTable = this.database.GetTable(tableName);
                        int[] leftTableIDs = leftTable.GetIDs();
                        int leftTableAttrIndex = leftTable.IndexOfAttribute(attribute);
                        if (leftTableAttrIndex == -1)
                            return null;

                        // Ausgabedatenbank erstellen und Ausgabetabelle verfügbarmachen
                        // Die Ausgabedatenbank hat eine Tabelle identisch zur linken verglichenen Tabelle
                        result = new Database(new Table[] { new Table(tableName, leftTable.GetAttributes(), new List<Record>()) });
                        Table resultTable = result.GetTable(tableName);

                        // Einträge mit dem String vergleichen und gleiche der Tabelle der Ausgabedatenbank hinzufügen
                        foreach (int id in leftTableIDs)
                        {
                            Record leftTableRecord = leftTable.Select(id);
                            if (leftTableRecord.GetValue(leftTableAttrIndex) == comp)
                            {
                                resultTable.Insert(leftTableRecord);
                            }
                        }

                        return result;

                    }
                    else
                    {

                    }

                    break;


            }



        }


    }
}
