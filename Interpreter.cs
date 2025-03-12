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

            // Conditions trennen
            string[] arr_conditions = conditions.Split(new string[] { " and ", " or " }, StringSplitOptions.None);

            // Ergebnisdatenbanken sammeln
            Database[] cond_results = new Database[arr_conditions.Length];
            for (int i = 0; i < arr_conditions.Length; i++)
            {
                cond_results[i] = Condition(arr_conditions[i]);
            }

            // Condition-Ergebnisse verknüpfen
            string[] args = conditions.Split(' ');

            int n_tables = cond_results.Length;
            Database result = new Database( new Table[n_tables] );

            for (int i = 0; i < arr_conditions.Length; i++)
            {
                if (i + 1 > arr_conditions.Length - 1) break;

                if (cond_results[i] == null) continue;

                if (arr_conditions[i+1] == " and ")
                {
                    Database left_db = cond_results[i];
                    List<Record> left_db_records = left_db.GetTable(0).GetRecords();
                    Database right_db = cond_results[i+1];
                    List<Record> right_db_records = right_db.GetTable(0).GetRecords();

                    Table res_table = new Table(i.ToString(), left_db.GetTable(0).GetAttributes());

                    foreach (Record r in left_db_records)
                    {
                        if (r == null) continue;
                        foreach (Record r2 in right_db_records)
                        {
                            if (r2 == null) continue;
                            if (r.Equals(r2))
                            {
                                res_table.Insert(r);
                            }
                        }

                    }

                    result.SetTable(i, res_table);

                }
                else if (arr_conditions[i+1] == " or ")
                {

                }
                else
                {
                    continue;
                }
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


            // Condition-Teile auslesen
            string tableName = args[0].Split('.')[0];
            string attribute = args[0].Split('.')[1];
            string op = args[1];

            // Rechten Teil wieder zusammensetzen
            string[] arr_comp = new string[args.Length - 2];
            Array.Copy(args, 2, arr_comp, 0, args.Length - 2);

            string comp = string.Join(" ", arr_comp);

            // Vergleich mit String oder anderem Attribut unterscheiden
            bool isString = comp.StartsWith("\"") && comp.EndsWith("\"");


            Database result;

            // Tabelle der linken Seite holen
            Table leftTable = this.database.GetTable(tableName);
            int[] leftTableIDs = leftTable.GetIDs();
            int leftTableAttrIndex = leftTable.IndexOfAttribute(attribute);
            if (leftTableAttrIndex == -1)
                return null;

            // Ggf. Tabelle der rechten Seite holen (sonst behalten die Variablen ihre Platzhalterwerte)
            Table rightTable = new Table("0", new string[] { });
            int[] rightTableIDs = new int[0];
            int rightTableAttrIndex = 0;
            if (!isString)
            {
                string[] rightArg = comp.Split('.');

                string rightTableName = rightArg[0];
                rightTable = this.database.GetTable(rightTableName);

                rightTableIDs = rightTable.GetIDs();

                string rightAttribute = rightArg[1];
                rightTableAttrIndex = rightTable.IndexOfAttribute(rightAttribute);
                if (rightTableAttrIndex == -1)
                    return null;
            }

            // Ausgabedatenbank erstellen und Ausgabetabelle verfügbar machen
            // Die Ausgabedatenbank hat eine Tabelle identisch zur linken verglichenen Tabelle
            result = new Database(new Table[] { new Table(tableName, leftTable.GetAttributes(), new List<Record>()) });
            Table resultTable = result.GetTable(tableName);


            // Vergleiche durchführen
            switch (op)
            {

                case "=":
                    if (isString)
                    {
                        // Einträge mit dem String vergleichen und gleiche der Tabelle der Ausgabedatenbank hinzufügen
                        foreach (int id in leftTableIDs)
                        {
                            Record leftTableRecord = leftTable.Select(id);
                            if (leftTableRecord.GetValue(leftTableAttrIndex) == comp)
                            {
                                resultTable.Insert(leftTableRecord);

                            }
                        }
                    }
                    else
                    {
                        // Jeden Eintrag mit jedem Eintrag der rechten Tabelle vergleichen und gleiche der Ausgabedatenbank hinzufügen
                        foreach (int lID in leftTableIDs)
                        {
                            Record leftTableRecord = leftTable.Select(lID);
                            foreach (int rID in rightTableIDs)
                            {
                                Record rightTableRecord = rightTable.Select(rID);
                                if (leftTableRecord.GetValue(leftTableAttrIndex) == rightTableRecord.GetValue(rightTableAttrIndex))
                                {
                                    resultTable.Insert(leftTableRecord);
                                }
                            }
                        }
                    }
                    return result;
                    

                case "!=":
                    if (isString)
                    {
                        // Einträge mit dem String vergleichen und ungleiche der Tabelle der Ausgabedatenbank hinzufügen
                        foreach (int id in leftTableIDs)
                        {
                            Record leftTableRecord = leftTable.Select(id);
                            if (leftTableRecord.GetValue(leftTableAttrIndex) != comp)
                            {
                                resultTable.Insert(leftTableRecord);

                            }
                        }
                    }
                    else
                    {
                        // Jeden Eintrag mit jedem Eintrag der rechten Tabelle vergleichen und gleiche der Ausgabedatenbank hinzufügen
                        foreach (int lID in leftTableIDs)
                        {
                            Record leftTableRecord = leftTable.Select(lID);
                            foreach (int rID in rightTableIDs)
                            {
                                Record rightTableRecord = rightTable.Select(rID);
                                if (leftTableRecord.GetValue(leftTableAttrIndex) != rightTableRecord.GetValue(rightTableAttrIndex))
                                {
                                    resultTable.Insert(leftTableRecord);
                                }
                            }
                        }
                    }
                    return result;

                // Aus: https://learn.microsoft.com/en-us/dotnet/api/system.string.compareto
                // Comparing 'some text' with '123': 1
                // Comparing 'some text' with 'some text': 0
                // Comparing 'some text' with 'Some Text': -1


                case ">":
                    if (isString)
                    {
                        // Einträge mit dem String vergleichen und größere der Tabelle der Ausgabedatenbank hinzufügen
                        foreach (int id in leftTableIDs)
                        {
                            Record leftTableRecord = leftTable.Select(id);
                            if (leftTableRecord.GetValue(leftTableAttrIndex).CompareTo(comp) > 0)
                            {
                                resultTable.Insert(leftTableRecord);

                            }
                        }
                    }
                    else
                    {
                        // Jeden Eintrag mit jedem Eintrag der rechten Tabelle vergleichen und größere der Ausgabedatenbank hinzufügen
                        foreach (int lID in leftTableIDs)
                        {
                            Record leftTableRecord = leftTable.Select(lID);
                            foreach (int rID in rightTableIDs)
                            {
                                Record rightTableRecord = rightTable.Select(rID);
                                if (leftTableRecord.GetValue(leftTableAttrIndex).CompareTo(rightTableRecord.GetValue(rightTableAttrIndex)) > 0)
                                {
                                    resultTable.Insert(leftTableRecord);
                                }
                            }
                        }
                    }
                    return result;


                case "<":
                    if (isString)
                    {
                        // Einträge mit dem String vergleichen und kleinere der Tabelle der Ausgabedatenbank hinzufügen
                        foreach (int id in leftTableIDs)
                        {
                            Record leftTableRecord = leftTable.Select(id);
                            if (leftTableRecord.GetValue(leftTableAttrIndex).CompareTo(comp) < 0)
                            {
                                resultTable.Insert(leftTableRecord);

                            }
                        }
                    }
                    else
                    {
                        // Jeden Eintrag mit jedem Eintrag der rechten Tabelle vergleichen und kleinere der Ausgabedatenbank hinzufügen
                        foreach (int lID in leftTableIDs)
                        {
                            Record leftTableRecord = leftTable.Select(lID);
                            foreach (int rID in rightTableIDs)
                            {
                                Record rightTableRecord = rightTable.Select(rID);
                                if (leftTableRecord.GetValue(leftTableAttrIndex).CompareTo(rightTableRecord.GetValue(rightTableAttrIndex)) < 0)
                                {
                                    resultTable.Insert(leftTableRecord);
                                }
                            }
                        }
                    }
                    return result;


                case ">=":
                    if (isString)
                    {
                        // Einträge mit dem String vergleichen und größere/gleiche der Tabelle der Ausgabedatenbank hinzufügen
                        foreach (int id in leftTableIDs)
                        {
                            Record leftTableRecord = leftTable.Select(id);
                            if (leftTableRecord.GetValue(leftTableAttrIndex).CompareTo(comp) >= 0)
                            {
                                resultTable.Insert(leftTableRecord);

                            }
                        }
                    }
                    else
                    {
                        // Jeden Eintrag mit jedem Eintrag der rechten Tabelle vergleichen und größere/gleiche der Ausgabedatenbank hinzufügen
                        foreach (int lID in leftTableIDs)
                        {
                            Record leftTableRecord = leftTable.Select(lID);
                            foreach (int rID in rightTableIDs)
                            {
                                Record rightTableRecord = rightTable.Select(rID);
                                if (leftTableRecord.GetValue(leftTableAttrIndex).CompareTo(rightTableRecord.GetValue(rightTableAttrIndex)) >= 0)
                                {
                                    resultTable.Insert(leftTableRecord);
                                }
                            }
                        }
                    }
                    return result;


                case "<=":
                    if (isString)
                    {
                        // Einträge mit dem String vergleichen und kleinere/gleiche der Tabelle der Ausgabedatenbank hinzufügen
                        foreach (int id in leftTableIDs)
                        {
                            Record leftTableRecord = leftTable.Select(id);
                            if (leftTableRecord.GetValue(leftTableAttrIndex).CompareTo(comp) <= 0)
                            {
                                resultTable.Insert(leftTableRecord);

                            }
                        }
                    }
                    else
                    {
                        // Jeden Eintrag mit jedem Eintrag der rechten Tabelle vergleichen und kleinere/gleiche der Ausgabedatenbank hinzufügen
                        foreach (int lID in leftTableIDs)
                        {
                            Record leftTableRecord = leftTable.Select(lID);
                            foreach (int rID in rightTableIDs)
                            {
                                Record rightTableRecord = rightTable.Select(rID);
                                if (leftTableRecord.GetValue(leftTableAttrIndex).CompareTo(rightTableRecord.GetValue(rightTableAttrIndex)) <= 0)
                                {
                                    resultTable.Insert(leftTableRecord);
                                }
                            }
                        }
                    }
                    return result;

            }

            return null;

                    


        }



    }


}

