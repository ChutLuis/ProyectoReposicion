using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;


namespace Reposición.Models
{
    public class Methods
    {
        // Variables
        //public static View GUI = new View();
        public static Table CurrentTable = null;
        public static List<string> TablesList = new List<string>();//Lista de Nombre de las Tablas
        public static string TreeFilePath = TreeFilePath_.Instance.a1;
        public static string TableFilePath = TableFilePath_.Instance.a1;
        public static string TraductionFilePath=Path_.Instance.a1;
        public static Syntax CodeSyntax = new Syntax();
        public static codeSQL OperationsSQL = new codeSQL();
        public static List<Row> Rows = new List<Row>();
        public static bool show = false;

        

        /// <summary>
        /// Metodos principales para el funcionamiento
        /// </summary>
        public static void DropTable(string tableName)
        {
            if (TablesList.Count > 0)
            {
                SetCurrentTable(TablesList[0]);
            }
            else
            {
                CurrentTable = null;
            }
            File.Delete(TableFilePath + tableName + ".tabla");
            File.Delete(TreeFilePath + tableName + ".txt");
        }

        public static void rowsData()
        {
            string columns = "";            
            for (int i = 0; i < CurrentTable.GetColumns().Count; i++)
            {
                columns += CurrentTable.GetColumns()[i].ColumnName + "|";
            }
            Rows = CurrentTable.SelectFrom(columns.Remove(columns.Length - 1));
            
        }



        public static void ExportCSV(string path)
        {
            StreamWriter writer = new StreamWriter(path);
            string line = "";
            for (int i = 0; i < Rows.Count(); i++)
            {
                for (int j = 0; j < CurrentTable.GetColumns().Count; j++)
                {
                    if (Methods.Rows[i].Registers[j] != null)
                    {
                        line += Methods.Rows[i].Registers[j].ToString() + ", ";
                    }
                    else
                    {
                        line += "~, ";
                    }
                }
                line = line.Remove(line.Length - 2) + Environment.NewLine;
            }
            writer.Write(line);
            writer.Flush();
            writer.Dispose();
            writer.Close();
        }



        public static bool SetCurrentTable(string tableName)
        {
            if (CurrentTable == null || CurrentTable.TableName != tableName)
            {
                if (TablesList.Find(x => x == tableName) != null)
                {
                    CurrentTable = new Table(tableName);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        public static void AddDataBaseItem(string tableName, string columnValues)
        {
            List<string> columnsValues = columnValues.Split('|').ToList();
            string txt = "";
            for (int i = 0; i < columnsValues.Count; i++)
            {
                if (columnsValues[i].Length > 25)
                {
                    txt += "» " + columnsValues[i].Remove(25) + "..." + Environment.NewLine;
                    continue;
                }
                txt += "» " + columnsValues[i] + Environment.NewLine;
            }
            instanceTableColumnsName.Instance.a1.Add(txt);
        }
        public static void CreateNewTable(string tableName, string columnValues)
        {
            CurrentTable = new Table(tableName, columnValues);
        }


        public static void FillReservatedWords(string path = null)
        {
            Syntax.ReservatedWords.Clear();
            string line = "";



            if (path != null && File.Exists(path))
            {
                StreamReader NewInstructions = new StreamReader(path);
                List<string> MainAndTraduction;
                while ((line = NewInstructions.ReadLine()) != null)
                {
                    if (line != "")
                    {
                        MainAndTraduction = line.Split(',').ToList();
                        Syntax.ReservatedWords.Add(new Instructions(MainAndTraduction[0].Trim().ToUpper(), MainAndTraduction[1].Trim().ToUpper()));
                    }
                }

                if (Syntax.ReservatedWords.Count != 11)
                {
                    NewInstructions.Close();
                    Message.Instance.a1 = "Archivo inválido, el archivo tiene más o menos instrucciones de las definidas en este gestor.\nSe seguirá trabajando con las instrucciones de serie.";
                    FillStandarWords();
                }
                else
                {
                    Message.Instance.a1 = "Idioma cambiado correctamente, el idioma del editor se ha cambiado correctamente.";
                }
                NewInstructions.Dispose();
                NewInstructions.Close();
            }
            else
            {
                FillStandarWords();
            }
            StreamWriter writer = new StreamWriter(TraductionFilePath);
            line = "";
            for (int i = 0; i < Syntax.ReservatedWords.Count; i++)
            {
                line += Syntax.ReservatedWords[i].Main + "," + Syntax.ReservatedWords[i].Traduction + Environment.NewLine;
            }
            writer.WriteLine(line);
            writer.Dispose();
            writer.Close();

        }
        public static void newRows(List<Row> nR)
        {
            Rows = nR;
        }

        private static void FillStandarWords()
        {
            Syntax.ReservatedWords.Add(new Instructions("SELECT", "SELECT"));
            Syntax.ReservatedWords.Add(new Instructions("FROM", "FROM"));
            Syntax.ReservatedWords.Add(new Instructions("DELETE", "DELETE"));
            Syntax.ReservatedWords.Add(new Instructions("WHERE", "WHERE"));
            Syntax.ReservatedWords.Add(new Instructions("CREATE TABLE", "CREATE TABLE"));
            Syntax.ReservatedWords.Add(new Instructions("DROP TABLE", "DROP TABLE"));
            Syntax.ReservatedWords.Add(new Instructions("INSERT INTO", "INSERT INTO"));
            Syntax.ReservatedWords.Add(new Instructions("VALUES", "VALUES"));
            Syntax.ReservatedWords.Add(new Instructions("GO", "GO"));
            Syntax.ReservatedWords.Add(new Instructions("UPDATE", "UPDATE"));
            Syntax.ReservatedWords.Add(new Instructions("SET", "SET"));
        }

        public static void ReviewInstructios(string richTextBoxText)
        {
            CodeSyntax.TextSintaxis(richTextBoxText);
        }
        public static bool OperateInstructions(string richTextBoxText)
        {
            List<string> SQLCode = CodeSyntax.TextSintaxis(richTextBoxText);
            if (SQLCode.Count == 0)
                return false;
            return OperationsSQL.Operate(0, SQLCode);

        }

    }
}
