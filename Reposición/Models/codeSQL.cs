using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Reposición.Models
{
    public class codeSQL
    {
        public bool Operate(int index, List<string> SQLCode)
        {
            string Error = "";
            switch (SQLCode[index].Split('|')[0])
            {
                case "SELECT":
                    List<Row> Results;
                    if (SQLCode.Count - index > 2 && SQLCode[index + 2].Split('|')[0] == "WHERE")
                    {
                        Results = Select(SQLCode[index++], SQLCode[index++], ref Error, SQLCode[index++]);
                    }
                    else
                    {
                        Results = Select(SQLCode[index++], SQLCode[index++], ref Error);
                    }
                    if (!SuccessOrError(Error))
                        return false;
                    Methods.show = true;
                    Methods.newRows(Results);
                    break;
                case "DELETE":
                    if (index + 2 < SQLCode.Count && SQLCode[index + 2].Split('|')[0] == "WHERE")
                    {
                        index++;
                        Delete(SQLCode[index++], ref Error, SQLCode[index++]);
                    }
                    else
                    {
                        Delete(SQLCode[++index], ref Error);
                    }
                    break;
                case "CREATE TABLE":
                    CreateTable(SQLCode[index++], ref Error);
                    break;
                case "DROP TABLE":
                    DropTable(SQLCode[index++], ref Error);
                    break;
                case "INSERT INTO":
                    InsertInTo(SQLCode[index++], SQLCode[index++], ref Error);
                    break;
                case "UPDATE":
                    Update(SQLCode[index++], SQLCode[index++], SQLCode[index++], ref Error);
                    break;
                default:
                    break;
            }

            if (!SuccessOrError(Error))
                return false;

            if (index >= SQLCode.Count - 1)
            {
                return true;
            }
            else
            {
                //es porque hay un go
                return Operate(++index, SQLCode);
            }

        }

        private List<Row> Select(string SelectLine, string FromLine, ref string Error, string WhereLine = null)
        {
            List<Column> Headers = new List<Column>();
            List<Row> Results = new List<Row>();
            string NameHeaders = "";

            if (!From(FromLine))
            {
                Error = "La tabla " + FromLine.Split('|')[1] + " no ha sido creada";
                return null;
            }

            //Valida si las columas existen
            if (SelectLine.Split('|')[1] == "*")
            {
                Headers = Methods.CurrentTable.GetColumns();
                for (int i = 0; i < Headers.Count; i++)
                {
                    NameHeaders += Headers[i].ColumnName + "|";
                }
                NameHeaders = NameHeaders.Remove(NameHeaders.Length - 1);
            }
            else
            {
                string[] ValidateHeaders = SelectLine.Split('|');
                for (int i = 1; i < ValidateHeaders.Length; i++)
                {
                    if (Methods.CurrentTable.GetColumns().Find(x => x.ColumnName.Equals(ValidateHeaders[i])) == null)
                    {
                        Error = "El nombre " + ValidateHeaders[i] + " No se encuentra dentro de la tabla " + Methods.CurrentTable.TableName;
                        return null;
                    }
                }
                NameHeaders = SelectLine.Remove(0, SelectLine.Split('|')[0].Length + 1); ;
            }

            //revisa si existe un where
            if (WhereLine != null)
            {
                if (Methods.CurrentTable.GetColumns().Find(x => x.ColumnName.Equals(WhereLine.Split('|')[1]) && x.IsPrimaryKey) == null)
                {
                    Error = "La tabla: " + Methods.CurrentTable.TableName + " no posee el campo: " + WhereLine.Split('|')[1] + " o bien este no es una llave primaria";
                    return null;
                }

                Row currentRow = Methods.CurrentTable.SelectFrom(NameHeaders, int.Parse(WhereLine.Split('|')[2]));
                if (currentRow == null)
                {
                    Error = "No se encontró el registro con la llave primaria igual a: " + WhereLine.Split('|')[1];
                    return null;
                }
                Results.Add(currentRow);
            }
            else
            {
                Results = Methods.CurrentTable.SelectFrom(NameHeaders);
            }

            return Results;
        }

        private bool From(string FromLine)
        {
            return Methods.SetCurrentTable(FromLine.Split('|')[1]);            
        }

       

        private bool Delete(string FromLine, ref string Error, string WhereLine = null)
        {
            if (!From(FromLine))
            {
                Error = "El nombre de la tabla " + FromLine.Split('|')[1] + " no existe";
                return false;
            }

            if (WhereLine != null)
            {
                if (!Methods.CurrentTable.Delete(int.Parse(WhereLine.Split('|')[2])))
                {
                    Error = "El elemento a borrar con la llave primaria igual a " + WhereLine.Split('|')[1] + " no existe";
                    return false;
                }
            }
            else
            {
                Methods.CurrentTable.Clear();
            }
            return true;
        }

        private bool CreateTable(string CreateTableLine, ref string Error)
        {
            if (Methods.TablesList.Find(x => x.Equals(CreateTableLine.Split('|')[1])) != null)
            {
                Error = "Ya existe una tabla con el nombre " + CreateTableLine.Split('|')[1];
                return false;
            }
            Methods.TablesList.Add(CreateTableLine.Split('|')[1]);
            Methods.CreateNewTable(CreateTableLine.Split('|')[1], CreateTableLine.Remove(0, CreateTableLine.Split('|')[0].Length + CreateTableLine.Split('|')[1].Length + 2));
            return true;
        }

        private bool DropTable(string DropLine, ref string Error)
        {
            if (!From(DropLine.Split('|')[0] + '|' + DropLine.Split('|')[1]))
            {
                Error = "No se ha registrado una tabla con el nombre de: " + DropLine.Split('|')[1];
                return false;
            }
            else
            {
                Methods.CurrentTable.Clear();
                int index = Methods.TablesList.FindIndex(x => x.Equals(DropLine.Split('|')[1]));
                if (index == -1)
                {
                    Error = "El nombre de la tabla: " + DropLine.Split('|')[1] + " no existe";
                }
                else
                {
                    Methods.TablesList.RemoveAt(index);
                    Methods.DropTable(DropLine.Split('|')[1]);
                }
            }
            return true;
        }

        private bool InsertInTo(string InsertLine, string ValuesLine, ref string Error)
        {
            string[] Columns = InsertLine.Split('|');
            string[] Values = ValuesLine.Split('|');

            if (!From(InsertLine.Split('|')[0] + "|" + InsertLine.Split('|')[1]))
            {
                Error = "No se ha guardado ninguna tabla con el nombre de: " + InsertLine.Split('|')[1];
                return false;
            }

            for (int i = 2; i < Columns.Length; i++)
            {
                Column CurrentColumn = Methods.CurrentTable.GetColumns().Find(x => x.ColumnName.Equals(Columns[i]));
                if (CurrentColumn == null)
                {
                    Error = "No existe la columna: " + Columns[i] + " en la tabla: " + InsertLine.Split('|')[1];
                    return false;
                }

                if (Values[i - 1].Replace("'", "").Length > CurrentColumn.MaxLength)
                {
                    Error = "El " + CurrentColumn.DataType + " " + Values[i - 1] + " posee un tamaño mayor a: " + CurrentColumn.MaxLength;
                }

                switch (CurrentColumn.DataType)
                {
                    case "VARCHAR":
                        if (Values[i - 1][0].ToString() != "'" || Values[i - 1][Values[i - 1].Length - 1].ToString() != "'")
                        {
                            Error = "El tipo VARCHAR " + Values[i - 1] + " no posee las comillas válidas";
                        }
                        break;
                    case "DATETIME":
                        if (Values[i - 1][0].ToString() != "'" || Values[i - 1][Values[i - 1].Length - 1].ToString() != "'")
                        {
                            Error = "El tipo DATETIME " + Values[i - 1] + " no posee las comillas válidas";
                        }
                        Values[i - 1] = Values[i - 1].Replace("'", "");
                        try
                        {
                            DateTime.Parse(Values[i - 1]);
                        }
                        catch
                        {
                            Error = "El dato " + Values[i - 1] + "  no posee el formato correcto de un tipo DATETIME";
                        }
                        break;
                    default: //cae acá para en int y el intprimary key
                        if (Values[i - 1].IndexOf("'") == 1)
                        {
                            Error = "El tipo INT " + Values[i - 1] + " no debe poseer comillas";
                        }
                        try
                        {
                            int.Parse(Values[i - 1]);
                        }
                        catch
                        {
                            Error = "El dato " + Values[i - 1] + "  no posee el formato correcto de un tipo INT";
                        }
                        break;
                }
            }
            Methods.CurrentTable.InsertInto(InsertLine.Remove(0, InsertLine.Split('|')[0].Length + 1 + InsertLine.Split('|')[1].Length + 1), ValuesLine.Replace("VALUES|", "").Replace("'", ""));
            return true;
        }

        private bool Update(string UpdateLine, string SetLine, string WhereLine, ref string Error)
        {
            //revisar update en sintaxis para que acepte más de un set
            string Columns = SetLine.Split('|')[1];
            string newValues = SetLine.Split('|')[2];


            if (!From(UpdateLine))
            {
                Error = "El nombre de la tabla " + UpdateLine.Split('|')[1] + " no existe";
                return false;
            }

            string[] EachColumn = Columns.Split('~');
            string[] EachValue = newValues.Split('~');
            Column CurrentColumn;
            for (int i = 0; i < EachColumn.Length; i++)
            {
                CurrentColumn = Methods.CurrentTable.GetColumns().Find(x => x.ColumnName.Equals(EachColumn[i]));
                if ((CurrentColumn) == null)
                {
                    Error = "En la " + UpdateLine.Split('|')[1] + " no existe una columna con el nombre de " + EachColumn[i];
                    return false;
                }

                if (CurrentColumn.IsPrimaryKey)
                {
                    Error = "No se puede editar: " + EachColumn[i] + " ya que en la tabla: " + UpdateLine.Split('|')[1] + " es llave primaria";
                    return false;
                }

                switch (CurrentColumn.DataType)
                {
                    case "INT":
                        if (EachValue[i].Length > CurrentColumn.MaxLength)
                        {
                            Error = "El dato: " + EachValue[i] + " de la columna: " + CurrentColumn.ColumnName + " supera el tamaño máximo de los INTS";
                            return false;
                        }
                        if (EachValue[i].IndexOf("'") != -1)
                        {
                            Error = "El dato INT: " + EachValue[i] + " de la columna: " + CurrentColumn.ColumnName + " posee comillas";
                            return false;
                        }
                        break;
                    case "VARCHAR":
                        if (EachValue[i].Replace("'", "").Length > CurrentColumn.MaxLength)
                        {
                            Error = "El dato: " + EachValue[i] + " de la columna: " + CurrentColumn.ColumnName + " supera el tamaño máximo. Este dato es: VARCHAR(" + CurrentColumn.MaxLength + ")";
                            return false;
                        }
                        if (EachValue[i][0].ToString() != "'" || EachValue[i][EachValue[i].Length - 1].ToString() != "'")
                        {
                            Error = "El dato VARCHAR(" + CurrentColumn.MaxLength + ")" + EachValue[i] + " de la columna: " + CurrentColumn.ColumnName + " no posee comillas válidas";
                            return false;
                        }
                        break;
                    case "DATETIME":
                        if (EachValue[i].Replace("'", "").Length > CurrentColumn.MaxLength)
                        {
                            Error = "El dato: " + EachValue[i] + " de la columna: " + CurrentColumn.ColumnName + " supera el tamaño máximo de los DATETIME";
                            return false;
                        }
                        if (EachValue[i][0].ToString() != "'" || EachValue[i][EachValue[i].Length - 1].ToString() != "'")
                        {
                            Error = "El dato DATETIME" + EachValue[i] + " de la columna: " + CurrentColumn.ColumnName + " no posee comillas válidas";
                            return false;
                        }
                        break;
                    default:
                        break;
                }
            }
            if (Methods.CurrentTable.GetColumns().Find(x => x.ColumnName.Equals(WhereLine.Split('|')[1]) && x.IsPrimaryKey) == null)
            {
                Error = "En la " + UpdateLine.Split('|')[1] + " no existe una columna con el nombre de " + SetLine.Split('|')[1] + " o bien no es una llave primaria";
                return false;
            }

            Methods.CurrentTable.Update(Columns, newValues.Replace("'", ""), int.Parse(WhereLine.Split('|')[2]));
            return true;
        }

        private bool SuccessOrError(string Error)
        {
            if (Error == "")
            {
                Message.Instance.a1="Listo";
                return true;
            }
            else
            {
                Message.Instance.a1="Error de compilación. Click aquí para más información";
                return false;
            }
        }
    }
}
