using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using StructurasDeDatos;

namespace Reposición.Models
{
    public class Table
    {
        private StreamReader reader;
        private StreamWriter writer;
        public string TableName { get; private set; }
        public static List<Column> Columns = new List<Column>();
        public B_Tree<int, Row> Rows;

        public Table(string Name, string columnsValues)
        {
            Column newColumn = new Column();
            List<string> columns = columnsValues.Split('|').ToList();
            for (int i = 0; i < columns.Count; i++)
            {
                List<string> values = columns[i].Split('~').ToList();
                if (values.Count == 3)
                {
                    newColumn = new Column(values[0], values[1], int.Parse(values[2]), false);
                }
                else
                {
                    if (values[1] == "DATETIME")
                    {
                        newColumn = new Column(values[0], values[1], 10, false);
                    }
                    else if (values[1] == "INT")
                    {
                        newColumn = new Column(values[0], "INT", 11, false);
                    }
                    else
                    {
                        newColumn = new Column(values[0], "INT", 11, true);
                    }
                }
                if (newColumn.IsPrimaryKey)
                {
                    Columns.Insert(0, newColumn);
                }
                else
                {
                    Columns.Add(newColumn);
                }
            }

            CreateFile(Name, columns);

            TableName = Name + ".txt";
            Rows = new B_Tree<int, Row>(3, TableName, Methods.TreeFilePath, x => int.Parse(x), x => AcceptData(x), ReturnData, 11, ReturnDataLength);
        }

        public Table(string tableFileName)
        {
            Columns.Clear();
            Column newColumn = new Column();

            reader = new StreamReader(Methods.TableFilePath + tableFileName + ".tabla");
            string line = "";
            TableName = reader.ReadLine();
            while ((line = reader.ReadLine()) != null)
            {
                List<string> values = line.Split('~').ToList();
                if (values.Count == 3)
                {
                    newColumn = new Column(values[0], values[1], int.Parse(values[2]), false);
                }
                else
                {
                    if (values[1] == "DATETIME")
                    {
                        newColumn = new Column(values[0], values[1], 10, false);
                    }
                    else if (values[1] == "INT")
                    {
                        newColumn = new Column(values[0], "INT", 11, false);
                    }
                    else
                    {
                        newColumn = new Column(values[0], "INT", 11, true);
                    }
                }
                if (newColumn.IsPrimaryKey)
                {
                    Columns.Insert(0, newColumn);
                }
                else
                {
                    Columns.Add(newColumn);
                }
            }
            Rows = new B_Tree<int, Row>(Methods.TreeFilePath, tableFileName + ".txt", x => int.Parse(x), x => AcceptData(x), ReturnData, 11, ReturnDataLength);
            reader.Dispose();
            reader.Close();
        }


        public List<Column> GetColumns()
        {
            return Columns;
        }

        private void CreateFile(string Name, List<string> columns)
        {
            string line = "";
            writer = new StreamWriter(Methods.TableFilePath + Name + ".tabla");
            line += Name + Environment.NewLine;
            for (int i = 0; i < columns.Count; i++)
            {
                line += columns[i] + Environment.NewLine;
            }

            writer.Write(line);
            writer.Flush();
            writer.Dispose();
            writer.Close();
        }

        public static IList<string> ReturnData(Row row)
        {
            IList<string> dataValues = new List<string>();
            for (int i = 0; i < row.Registers.Count; i++)
            {
                dataValues.Add(row.Registers[i]);
            }
            return dataValues;
        }

        public static IList<int> ReturnDataLength()
        {
            IList<int> dataLength = new List<int>();
            for (int i = 0; i < Columns.Count; i++)
            {
                dataLength.Add(Columns[i].MaxLength);
            }
            return dataLength;
        }

        public static Row AcceptData(string data)
        {
            Row register = new Row(Columns.Count);
            register.Registers = data.Split('#').ToList();
            return register;
        }

        public Row InsertInto(string columnsNames, string dataValues)
        {
            // create a new row
            Row row = new Row(Columns.Count);

            List<string> columns = columnsNames.Split('|').ToList(); 
            List<string> values = dataValues.Split('|').ToList(); 

            for (int i = 0; i < Columns.Count; i++)
            {
                if (columns.Count > i)
                {
                    int index = Columns.FindIndex(x => x.ColumnName.Equals(columns[i]));
                    row.Registers[index] = values[i];
                }
                else
                {
                    break;
                }
            }
            if (Rows.Search(int.Parse(row.Registers[0])) == null)
            {
                Rows.Insert(int.Parse(row.Registers[0]), row);
                return row;
            }
            else
            {
                return new Row(Columns.Count);
            }

        }

        public Row Search(int id)
        {
            if (Rows.Search(id) != null)
            {
                List<Row> registers = Rows.Search(id).NodeData;
                return registers.Find(x => x.Registers[0] == id.ToString());
            }
            return null;
        }

        public Row SelectFrom(string columnsNames, int id)
        {
            Row row = Search(id);
            if (row != null)
            {
                List<string> columns = columnsNames.Split('|').ToList(); 

                for (int i = 0; i < Columns.Count; i++)
                {
                    if (columns.Find(x => x == Columns[i].ColumnName) == null)
                    {
                        row.Registers[i] = "";
                    }
                }
                return row;
            }
            return new Row(Columns.Count);
        }

        public List<Row> SelectFrom(string columnsNames)
        {
            Row newRow;
            List<Row> data = new List<Row>();
            List<string> dataValues = new List<string>();
            dataValues = Rows.InOrder(Rows.Root, dataValues);
            List<string> columnsSelected = columnsNames.Split('|').ToList();
            for (int i = 0; i < dataValues.Count; i++)
            {
                var line = dataValues[i].Split('|');
                newRow = new Row(Columns.Count);
                for (int j = 0; j < columnsSelected.Count; j++)
                {
                    newRow.Registers[Columns.FindIndex(x => x.ColumnName.Equals(columnsSelected[j]))] = dataValues[i].Split('|')[j];
                }
                data.Add(newRow);
            }
            return data;
        }

        public Row Update(string columnsNames, string dataValues, int id)
        {
            if (Rows.Search(id) != null)
            {
                int index = Rows.Search(id).NodeKeys.IndexOf(id);
                List<Row> treeDataValues = Rows.Search(id).NodeData;

                List<string> columns = columnsNames.Split('|').ToList(); 
                List<string> values = dataValues.Split('|').ToList(); 

                for (int i = 0; i < columns.Count; i++)
                {
                    if (Columns.Find(x => x.ColumnName == columns[i]) != null)
                    {
                        treeDataValues[index].Registers[Columns.FindIndex(x => x.ColumnName == columns[i])] = values[i];
                    }
                }
                Rows.Update(treeDataValues, id);
                return treeDataValues[index];
            }
            return new Row(Columns.Count);
        }

        public bool Delete(int key)
        {
            try
            {
                Rows.Delete(key);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Clear()
        {
            Rows.Clear();
        }
    }
}
