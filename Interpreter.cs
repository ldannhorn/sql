﻿using System;
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
            (not) table_name.attribute = "string"/table_name.attribute
            (not) table_name.attribute != "string"/table_name.attribute
            (not) table_name.attribute > "string"/table_name.attribute
            (not) table_name.attribute < "string"/table_name.attribute
            (not) table_name.attribute >= "string"/table_name.attribute
            (not) table_name.attribute <= "string"/table_name.attribute
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
            (not) table_name.attribute = "string"/table_name.attribute
            (not) table_name.attribute != "string"/table_name.attribute
            (not) table_name.attribute > "string"/table_name.attribute
            (not) table_name.attribute < "string"/table_name.attribute
            (not) table_name.attribute >= "string"/table_name.attribute
            (not) table_name.attribute <= "string"/table_name.attribute
            */

            string[] args = condition.Split(' ');

            // "not" vorhanden? -> Index verschieben?
            int indexOffset = 0;
            bool not = false;
            if (args[0] == "not")
            {
                indexOffset = 1;
                not = true;
            }

            // Condition-Teile auslesen
            string tableName = args[0 + indexOffset].Split('.')[0 + indexOffset];
            string attribute = args[0 + indexOffset].Split('.')[1 + indexOffset];
            string op = args[1 + indexOffset];

            // Rechten Teil wieder zusammensetzen
            string[] arr_comp = new string[args.Length - 2 - indexOffset];
            Array.Copy(args, 2 + indexOffset, arr_comp, 0, args.Length - 2 - indexOffset);

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
                                if (leftTableRecord.GetValue(leftTableAttrIndex).CompareTo(rightTableRecord.GetValue(rightTableAttrIndex)) < 0)
                                {
                                    resultTable.Insert(leftTableRecord);
                                }
                            }
                        }
                    }
                    return result;


            }

                    


        }



    }


}

