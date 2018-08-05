using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Reposición.Models
{
    public class Syntax
    {
        
        public static List<Instructions> ReservatedWords = new List<Instructions>();
        public static List<string> DataType = new List<string>();

        public List<string> SeparateInstructions(string textToSeparated)
        {
            List<string> AllTheInstrucctions = new List<string>();
            if (textToSeparated.IndexOf('|') != -1)
            {
                Message.Instance.a1="Error de sintaxis, no se puede ingresar ningún caracter |";
                return AllTheInstrucctions;
            }
            textToSeparated = textToSeparated.ToUpper();
            List<string> WordToWord = (textToSeparated.Trim()).Split().ToList();
            WordToWord.RemoveAll(x => x.Equals(""));
            string Instruction = "";

            for (int i = 0; i < WordToWord.Count; i++)
            {
                Instructions CorrectSingleWord = ReservatedWords.Find(x => x.Traduction == WordToWord[i].ToUpper());
                if (CorrectSingleWord != null)
                {
                    AllTheInstrucctions.Add(Instruction);
                    Instruction = (CorrectSingleWord.TraslateToMain(WordToWord[i]));
                }
                else
                {
                    string concat = "";
                    Instructions CorrectInstruction;
                    if ((CorrectInstruction = ReservatedWords.Find(x => x.Traduction.Split()[0] == WordToWord[i].ToUpper())) != null)
                    {
                        AllTheInstrucctions.Add(Instruction);
                        concat = WordToWord[i].ToUpper();
                        for (int k = 1; k < CorrectInstruction.Traduction.Split().Length + 1; k++)
                        {
                            if ((CorrectSingleWord = ReservatedWords.Find(x => x.Traduction == concat.ToUpper())) != null)
                            {
                                //encontró una palabra doble o triple 
                                Instruction = CorrectSingleWord.Main;
                            }
                            else
                            {
                                if (k == CorrectInstruction.Traduction.Split().Length)
                                {
                                    //error
                                    Message.Instance.a1="Error de sintaxis, se ha introducido una instruccion incorrecta: " + concat + ". ¿Quizás quiso indicar: " + CorrectInstruction.Traduction + "? ";
                                    AllTheInstrucctions.Clear();
                                    return AllTheInstrucctions;
                                }
                                else
                                {
                                    concat += " " + WordToWord[i + k];
                                    i++;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (i > 0 && WordToWord[i - 1].Trim() == "VARCHAR")
                        {
                            Instruction += "" + WordToWord[i];
                        }
                        else
                        {
                            Instruction += "|" + WordToWord[i];
                        }

                    }
                }
            }
            AllTheInstrucctions.Add(Instruction);
            AllTheInstrucctions.RemoveAll(x => x == "");
            Message.Instance.a1="Listo";
            return AllTheInstrucctions;
        }

        public List<string> TextSintaxis(string textToSeparated)
        {
            List<string> Lines = SeparateInstructions(textToSeparated);
            string ContainTemporal = "";
            int amountInstructionInsert_Values = 0;
            string Error = "";
            string Chain = "";

            if (Lines.Count == 0)
                return Lines;
            try
            {
                for (int i = 0; i < Lines.Count; i++)
                {
                    string[] LineParts = Lines[i].Split('|');

                    switch (LineParts[0])
                    {
                        case "SELECT":
                            if (LineParts.Length >= 2 && LineParts.Length <= 10)
                            {
                                //error de sintaxis
                                //no se puede encontrar una tabla con más o menos de dos palabras

                                ContainTemporal = Lines[i];
                                ContainTemporal = ContainTemporal.Replace("SELECT|", "");
                                ContainTemporal = ContainTemporal.Replace("|", "");
                                string[] columns = ContainTemporal.Split(',');
                                for (int l = 0; l < columns.Length; l++)
                                {
                                    if (columns[l].Split().Length > 1)
                                    {
                                        //error de sintaxis
                                        Message.Instance.a1="Error de sintaxis, se ha introducido una instrucción " + Traslate(false, LineParts[0]) + " inválido. \n" + Lines[i].Replace('|', ' ').Remove(0, "SELECT".Length) + ". \nAlguno de los campos tiene más de una palabra";
                                        Lines.Clear();
                                        return Lines;
                                    }
                                }
                                Lines[i] = Lines[i].Replace(",", "");
                            }
                            else
                            {
                                Message.Instance.a1="Error de sintaxis, se ha introducido una instrucción " + Traslate(false, LineParts[0]) + " inválido.\n" + Lines[i].Replace('|', ' ').Remove(0, "SELECT".Length);
                                Lines.Clear();
                                return Lines;
                            }
                            break;
                        case "FROM":
                            if (LineParts.Length != 2)
                            {
                                //error de sintaxis
                                //no se puede encontrar una tabla con más o menos de dos palabras
                                Message.Instance.a1="Error de sintaxis, se ha introducido una instrucción " + Traslate(false, LineParts[0]) + " inválida.\n" + Lines[i].Replace('|', ' ').Remove(0, "FROM".Length);
                                Lines.Clear();
                                return Lines;
                            }
                            break;
                        case "DELETE":
                            //validar lenght
                            if (Lines.Count != 1)
                            {
                                if (Lines[i + 1].Split('|')[0] != "FROM")
                                {
                                    //error de sintaxis
                                    //No se entiende de que tabla hay que borrar
                                    Message.Instance.a1="Error de sintaxis, se ha introducido una instrucción " + Traslate(false, LineParts[0]) + " inválida.\n" + Lines[i + 1].Replace('|', ' ');
                                    Lines.Clear();
                                    return Lines;
                                }
                            }
                            else
                            {
                                Message.Instance.a1 = "Error de sintaxis, se ha introducido una instrucción " + Traslate(false, LineParts[0]) + " inválida. Instrucción considerada incompleta";
                                Lines.Clear();
                                return Lines;
                            }
                            break;
                        case "WHERE":
                            Error = "";
                            Chain = "";
                            if (!ValidateConditions(Lines[i], Lines[i].Replace("WHERE", ""), ref Error, ref Chain,true))
                            {
                                Lines.Clear();
                                Message.Instance.a1 = "Error de sintaxis" + Error + " :" + Lines[i].Replace("WHERE", "");
                                return Lines;
                            }
                            Lines[i] = Chain;
                            break;
                        case "CREATE TABLE":
                            int existIntPrimaryKey = 0;
                            if (((LineParts[2][0] == '(' ^ LineParts[1][LineParts[1].Length - 1] == '(' ^ DoesParenthesisExistInTheMiddle(LineParts[1])) && (ValidateLastestParentesis(LineParts) ^ LineParts[LineParts.Length - 1] == ")")))
                            {
                                if (LineParts[1].IndexOf('(') != -1)
                                {
                                    if (LineParts[1].Split('(').Length > 2)
                                    {
                                        Lines.Clear();
                                        Message.Instance.a1 = "Error de sintaxis, rxisten paréntesis inválidos en la expresión " + Traslate(false, "CREATE TABLE");
                                        return Lines;
                                    }
                                }
                                string[] auxArray = DeleteParenthesis(Lines[i], LineParts, false).Replace('|', ' ').Split(',');
                                string auxString = "";
                                string varcharlength = "";
                                Error = "";

                                for (int m = 0; m < auxArray.Length; m++)
                                {
                                    auxArray[m] = auxArray[m].Trim();;
                                    if ((DataType.Find(x => x == auxArray[m].Split()[0]) == null) && (((DataType.Find(x => x == auxArray[m].Remove(0,auxArray[m].Split()[0].Length+1).Trim())) != null) || (RecognizeVarchar(auxArray[m].Replace(auxArray[m].Split()[0], "").Trim()))))
                                    {
                                        if (!ValidateNameAndValueSyntax(auxArray[m].Split()[0]))
                                        {
                                            Message.Instance.a1 = "Error de sintaxis, se ha introducido un nombre con caracteres inválidos: " + auxArray[m].Split()[0];
                                            Lines.Clear();
                                            return Lines;
                                        }
                                        if (!PurifyVarchar(auxArray[m].Replace(auxArray[m].Split()[0], "").Trim(), ref varcharlength, ref Error))
                                        {
                                            Message.Instance.a1 = "Error de sintaxis" + Error;
                                            Lines.Clear();
                                            return Lines;
                                        }

                                        if (auxArray[m].Replace(auxArray[m].Split()[0], "").IndexOf('(') != -1)
                                        {
                                            auxString += auxArray[m].Split()[0].ToUpper().Trim() + "~" + "VARCHAR" + varcharlength + "|";
                                        }
                                        else
                                        {
                                            auxString += auxArray[m].Split()[0].ToUpper().Trim() + "~" + auxArray[m].Remove(0, auxArray[m].Split()[0].Length + 1).Trim() + "|";
                                        }

                                        if (auxArray[m].Remove(0, auxArray[m].Split()[0].Length + 1).Trim().ToUpper().Trim().ToUpper() == "INT PRIMARY KEY")
                                        {
                                            existIntPrimaryKey++;
                                        }
                                    }
                                    else
                                    {
                                        //error de sintaxis
                                        //ya que no se tiene un nombre de una variable y un tipo de dato, se tiene algo más
                                        Message.Instance.a1 = "Error de sintaxis, se ha introducido incorrectamente condicion de la instrucción " + Traslate(false, LineParts[0]) + ".\n" + Lines[i].Replace('|', ' ').Remove(0, "CREATE TABLE".Length) + "\n Se espera siempre una coma, a excepción del elemento final";
                                        Lines.Clear();
                                        return Lines;
                                    }
                                }

                                if (existIntPrimaryKey != 1)
                                {
                                    Message.Instance.a1 = "Error de sintaxis, se ha introducido incorrectamente la instrucción " + Traslate(false, LineParts[0]) + ". \nHay más de una o ninguna llave primaria";
                                    Lines.Clear();
                                    return Lines;
                                }
                                Lines[i] = (LineParts[0] + "|" + LineParts[1].Split('(')[0] + "|" + auxString.Remove(auxString.Length - 1)).Replace("||", "|");
                            }
                            else
                            {
                                //error de sintaxis
                                Message.Instance.a1 = "Error de sintaxis, se ha introducido incorrectamente la instrucción " + Traslate(false, LineParts[0]) + ": Es probable que que existan más o menos de dos paréntesis, o bien que no se haya especificado el nombre de la tabla a crear";
                                Lines.Clear();
                                return Lines;
                            }
                            break;
                        case "DROP TABLE":
                            if (LineParts.Length != 2)
                            {
                                //error de sintaxis
                                //No se puede encontrar una tabla con más o menos de dos palabras por nombres
                                Message.Instance.a1 = "Error de sintaxis, se ha introducido incorrectamente en la instrucción " + Traslate(false, LineParts[0]) + "los nombres de tablas : " + Lines[i].Replace('|', ' ').Remove(0, "DROP TABLE".Length);
                                Lines.Clear();
                                return Lines;
                            }
                            break;
                        case "INSERT INTO":
                            if ((LineParts[2][0] == '(' ^ LineParts[1][LineParts[1].Length - 1] == '(' ^ DoesParenthesisExistInTheMiddle(LineParts[1])) && (ValidateLastestParentesis(LineParts) ^ LineParts[LineParts.Length - 1] == ")"))
                            {
                                if (LineParts[1].IndexOf('(') != -1)
                                {
                                    if (LineParts[1].Split('(').Length > 2)
                                    {
                                        Lines.Clear();
                                        Message.Instance.a1 = "Error de sintaxis, existen paréntesis inválidos en la expresión " + Traslate(false, "CREATE TABLE");
                                        return Lines;
                                    }
                                }
                                string[] auxArray = DeleteParenthesis(Lines[i], LineParts, false).Replace('|', ' ').Split(',');
                                string auxString = "";
                                for (int m = 0; m < auxArray.Length; m++)
                                {
                                    if (!ValidateNameAndValueSyntax(auxArray[m]))
                                    {
                                        Message.Instance.a1 = "Error de sintaxis, se ha introducido un nombre con caracteres inválidos: " + auxArray[m];
                                        Lines.Clear();
                                        return Lines;
                                    }
                                    auxString += auxArray[m].Trim() + "|";
                                }

                                Lines[i] = (LineParts[0] + "|" + LineParts[1].Split('(')[0] + "|" + auxString.Remove(auxString.Length - 1)).Replace("||", "|");
                                amountInstructionInsert_Values = auxArray.Length;
                            }
                            else
                            {
                                //error de sintaxis
                                Message.Instance.a1 = "Error de sintaxis, se ha introducido incorrectamente la instrucción " + Traslate(false, LineParts[0]) + "\nRevise que la instrucción termine y empiece con parentesis, cerciorarse que no se esten ingresando más campos de los permitidos";
                                Lines.Clear();
                                return Lines;
                            }
                            break;
                        case "VALUES":
                            if (((LineParts[1][0] == '(' ^ LineParts[0][LineParts[0].Length - 1] == '(' ^ DoesParenthesisExistInTheMiddle(LineParts[0])) && (ValidateLastestParentesis(LineParts) ^ LineParts[LineParts.Length - 1] == ")")))
                            {
                                if (LineParts[1].IndexOf('(') != -1)
                                {
                                    if (LineParts[1].Split('(').Length > 2)
                                    {
                                        Lines.Clear();
                                        Message.Instance.a1 = "Error de sintaxis, existen paréntesis inválidos en la expresión " + Traslate(false, "CREATE TABLE");
                                        return Lines;
                                    }
                                }
                                string[] auxArray = DeleteParenthesis(Lines[i], LineParts, true).Replace('|', ' ').Split(',');
                                if (auxArray.Length != amountInstructionInsert_Values)
                                {
                                    Lines.Clear();
                                    Message.Instance.a1 = "Error de sintaxis, se ha introducido incorrectamente la instrucción " + Traslate(false, LineParts[0]) + "\nExisten más o menos valores ingresados en la instrucción " + Traslate(false, "VALUES") + " que los declarados en la instrucción " + Traslate(false, "INSERT INTO");
                                    return Lines;
                                }
                                string auxString = "";
                                for (int m = 0; m < auxArray.Length; m++)
                                {
                                    if (!ValidateNameAndValueSyntax(auxArray[m]))
                                    {
                                        Message.Instance.a1 = "Error de sintaxis, se ha introducido un dato con caracteres inválidos: " + auxArray[m];
                                        Lines.Clear();
                                        return Lines;
                                    }
                                    auxString += auxArray[m].Trim() + "|";
                                }

                                Lines[i] = (LineParts[0] + "|" + LineParts[1].Split('(')[0] + "|" + auxString.Remove(auxString.Length - 1)).Replace("||", "|");
                            }
                            else
                            {
                                Message.Instance.a1 = "Error de sintaxis, se ha introducido incorrectamente la instrucción " + Traslate(false, LineParts[0]) + ": Revise que la instrucción termine y empiece con parentesis, serciorarse que no se esten ingresando más campos de especificados en la instrucción " + Traslate(false, "INSERT INTO");
                                Lines.Clear();
                                return Lines;

                            }
                            break;
                        case "GO":
                            if (LineParts.Length == 1)
                            {
                                if (i + 1 >= Lines.Count)
                                {
                                    Message.Instance.a1 = "Error de sintaxis, se ha introducido incorrectamente la instrucción " + Traslate(false, LineParts[0]) + ".\nNo existe una instrucción siguiente a ejecutar.\n" + Lines[i].Replace('|', ' ').Remove(0, "GO".Length);
                                    Lines.Clear();
                                    return Lines;
                                }
                                if (ReservatedWords.Find(x => x.Traduction.Equals(Lines[i + 1].Split('|')[0])) == null)
                                {
                                    Message.Instance.a1 = "Error de sintaxis, se ha introducido incorrectamente la instrucción " + Traslate(false, LineParts[0]) + ".\nNo existe una instrucción siguiente a ejecutar.\n" + Lines[i].Replace('|', ' ').Remove(0, "GO".Length);
                                    Lines.Clear();
                                    return Lines;
                                }
                            }
                            else
                            {
                                Message.Instance.a1 = "Error de sintaxis, se ha introducido incorrectamente la instrucción " + Traslate(false, LineParts[0]) + ".\nSe han introducido argumentos demás .\n" + Lines[i].Replace('|', ' ').Remove(0, "GO".Length);
                                Lines.Clear();
                                return Lines;
                            }
                            break;
                        default:
                            //error de sintaxis ya que no inicia con una instrucción
                            Message.Instance.a1 = "Error de sintaxis, bloque de instrucciones no inicia con una palabra reservada";
                            Lines.Clear();
                            return Lines;
                            break;

                        case "UPDATE":
                            if (LineParts.Length!=2)
                            {
                                Message.Instance.a1 = "Error de sintaxis, la instrucción "+Traslate(false,"UPDATE")+" tiene argumentos inválidos: "+Lines[i].Replace("UPDATE","");
                                Lines.Clear();
                                return Lines;
                            }
                            break;
                        case "SET":
                            Error = "";
                            Chain = "";
                            if (!ValidateConditions(Lines[i], Lines[i].Replace("SET", ""), ref Error, ref Chain, false))
                            {
                                Lines.Clear();
                                Message.Instance.a1 = "Error de sintaxis " + Error + " :" + Lines[i].Replace("SET", "");
                                return Lines;
                            }
                            Lines[i] = Chain;
                            break;
                    }
                }
            }
            catch
            {
                //error de sintaxis
                Message.Instance.a1 = "Error de sintaxis, se ha introducido una instrucción inválida, si posee alguna duda, puede consultar la ayuda";
                Lines.Clear();
                return Lines;
            }

            int u = 0;
            if (!InstrucctionOrder(Lines, u))
            {
                Lines.Clear();
            }
            else
            {
                Message.Instance.a1 = "Listo";
            }
            return Lines;

        }//final de metodo

        public bool InstrucctionOrder(List<string> CodeLines, int index)
        {
            try
            {
                switch (CodeLines[index].Split('|')[0])
                {
                    case "SELECT":
                        if (CodeLines[++index].Split('|')[0] != "FROM")
                        {
                            Message.Instance.a1 = "Error de sintaxis, la instrucción " + Traslate(false, "SELECT") + " espera un " + Traslate(false, "FROM");
                            return false;
                        }

                        if (index < CodeLines.Count - 1)
                        {
                            if (CodeLines[index + 1].Split('|')[0] != "WHERE" && CodeLines[index + 1].Split('|')[0] != "GO")
                            {
                                Message.Instance.a1 = "Error de sintaxis, la instrucción " + Traslate(false, "SELECT") + " espera un " + Traslate(false, "FROM");
                                return false;
                            }
                            if (CodeLines[index + 1].Split('|')[0] == "WHERE")
                                index++;
                        }
                        break;
                    case "DELETE":
                        if (CodeLines[++index].Split('|')[0] != "FROM")
                        {
                            Message.Instance.a1 = "Error de sintaxis. Click aquí para más información, la instrucción " + Traslate(false, "DELETE") + " espera un " + Traslate(false, "FROM");
                            return false;
                        }

                        if (index+1<CodeLines.Count  &&  CodeLines[++index].Split('|')[0] != "WHERE")
                        {
                            Message.Instance.a1 = "Error de sintaxis, la instrucción " + Traslate(false, "FROM") + " espera un " + Traslate(false, "WHERE");
                            return false;
                        }
                        break;
                    case "CREATE TABLE":
                        break;
                    case "DROP TABLE":
                        break;
                    case "INSERT INTO":
                        if (CodeLines[++index].Split('|')[0] != "VALUES")
                        {
                            Message.Instance.a1 = "Error de sintaxis, la instrucción " + Traslate(false, "INSERT INTO") + " espera un " + Traslate(false, "VALUES");
                            return false;
                        }
                        break;
                    case "UPDATE":
                        if (CodeLines[++index].Split('|')[0] != "SET")
                        {
                            Message.Instance.a1 = "Error de sintaxis, la instrucción " + Traslate(false, "UPDATE") + " espera un " + Traslate(false, "SET");
                            return false;
                        }
                        else if (CodeLines[++index].Split('|')[0] != "WHERE")
                        {
                            Message.Instance.a1 = "Error de sintaxis, la instrucción " + Traslate(false, "SET") + " espera un " + Traslate(false, "WHERE");
                            return false;
                        }
                        break;
                    default:
                        Message.Instance.a1 = "Error de sintaxis, usted ha ingresado una instrucción incompleta: " + CodeLines[index].Replace('|', ' ').Replace('~', ' ').Replace(CodeLines[index].Split()[0], Traslate(false, CodeLines[index].Split()[0]));
                        return false;
                        break;
                }
            }
            catch
            {
                Message.Instance.a1 = "Error de sintaxis, se ha ingresado una secuencia de instrucciones incompletas o inválida";
                return false;
            }

            if (index == CodeLines.Count - 1)
            {
                return true;
            }
            else
            {
                if (CodeLines[++index].Split('|')[0] == "GO")
                    return InstrucctionOrder(CodeLines, ++index);
            }

            Message.Instance.a1 = "Error de sintaxis, se ha ingresado una secuencia de instrucciones imposibles de editar";
            return false;
        }

        private string DeleteParenthesis(string Line, string[] LineParts, bool IsForValues)
        {
            int less = 0;
            if (IsForValues)
            {
                less++;
            }

            if (LineParts[2 - less][0] == '(' && LineParts[2 - less].Length > 1)
            {
                if (IsForValues)
                {
                    Line = Line.Remove(0, LineParts[0].Length + 2);
                }
                else
                {
                    Line = Line.Remove(0, LineParts[0].Length + LineParts[1].Length + 3);
                }
            }
            else
            {
                if (LineParts[1 - less][LineParts[1 - less].Length - 1] == '(')
                {
                    if (IsForValues)
                    {
                        Line = Line.Remove(0, LineParts[0].Length + 1);
                    }
                    else
                    {
                        Line = Line.Remove(0, LineParts[0].Length + 1 + LineParts[1].Length);
                    }
                }
                else if (LineParts[2 - less] == "(")
                {
                    if (IsForValues)
                    {
                        Line = Line.Remove(0, LineParts[0].Length + LineParts[1].Length + 2);
                    }
                    else
                    {
                        Line = Line.Remove(0, LineParts[0].Length + LineParts[1].Length + LineParts[2].Length + 3);
                    }
                }
                else if (LineParts[1].IndexOf('(') != -1)
                {
                    Line = Line.Replace(LineParts[1], LineParts[1].Split('(')[0] + LineParts[1].Split('(')[1]);
                    if (IsForValues)
                    {
                        Line = Line.Remove(0, LineParts[0].Length + 1);
                    }
                    else
                    {
                        Line = Line.Remove(0, LineParts[0].Length + 1 + LineParts[1].Split('(')[0].Length);
                    }
                }
            }
            Line = Line.Remove(Line.Length - 1, 1);
            return Line;
        }

        private bool ValidateLastestParentesis(string[] LineParts)
        {
            if (LineParts[LineParts.Length - 1].Length == 1)
            {
                return false;
            }
            else
            {
                return LineParts[LineParts.Length - 1][LineParts[LineParts.Length - 1].Length - 1] == ')';
            }
        }

        private bool DoesParenthesisExistInTheMiddle(string LinePart)
        {
            if (LinePart.IndexOf('(') != -1)
            {
                string[] s = LinePart.Split('(');
                if (s[1] != "")
                {
                    return true;
                }
            }

            return false;
        }

        private bool ValidateNameAndValueSyntax(string name)
        {
            for (int i = 0; i < name.Length; i++)
            {
                if ("+-*~^=¬¿?¡![]{}<>()¨`´&%$#@|°;.:,".IndexOf(name[i]) != -1 || name[i] == '\"' || name[i].ToString() == @"\")
                {
                    return false;
                }
            }
            return true;
        }

        private bool ValidateConditions(string whereLine, string condition, ref string Error, ref string chain, bool IsWhere)
        {
            List<string> ConditionParts;
            if (condition.IndexOf('!') == -1)
            {
                if (IsWhere)
                {
                    condition = condition.Replace('|', ' ');
                    ConditionParts = condition.Split('=').ToList();
                    ConditionParts.RemoveAll(x => x.Equals("") || x.Equals(" "));
                    if (ConditionParts.Count != 2)
                    {
                        Error = "La condición "+ Traslate(false, "WHERE")+" inválida, asegurese que la condición y cualquier instrucción que vaya después tenga la sintaxis correcta";
                        return false;
                    }
                    try
                    {
                        int.Parse(ConditionParts[1].Trim());
                    }
                    catch
                    {
                        Error = "La condición inválida del " + Traslate(false, "WHERE") + ": " + whereLine.Replace("WHERE", "");
                        return false;
                    }
                    chain = "WHERE|" + ConditionParts[0].Trim() + "|" + ConditionParts[1].Trim();
                }
                else
                {
                    string ColumnsNames = "", ColumnsValues ="";
                    condition = condition.Replace('|', ' ');
                    ConditionParts = condition.Split(',').ToList();

                    try
                    {
                        for (int i = 0; i < ConditionParts.Count; i++)
                        {
                            ColumnsNames += ConditionParts[i].Split('=')[0].Trim()+"~";
                            ColumnsValues += ConditionParts[i].Split('=')[1].Trim()+"~";
                        }
                       ColumnsNames = ColumnsNames.Remove(ColumnsNames.Length-1);
                       ColumnsValues = ColumnsValues.Remove(ColumnsValues.Length-1);
                    }
                    catch
                    {
                        Error = "condición " + Traslate(false, "SET") + " inválida, asegurese que la condición y cualquier instrucción que vaya después tenga la sintaxis correcta";
                        return false;
                    }
                    chain = "SET|" + ColumnsNames + "|" + ColumnsValues;
                }
            }
            return true;
        }

        private bool RecognizeVarchar(string Instance)
        {
            string type = Instance.Replace("(", " ").Replace(")", "");
            string[] VarcharParts = type.Split();
            if (VarcharParts[0] == "VARCHAR")
            {
                return true;
            }
            return false;
        }

        private bool PurifyVarchar(string instance, ref string varcharlength, ref string Error)
        {
            string type = instance.Replace("(", " ").Replace(")", "");
            string[] VarcharParts = type.Split();


            if (VarcharParts[0] != "VARCHAR")
            {
                varcharlength = "";
                return true;
            }

            if (instance["VARCHAR".Length] != '(' || instance[instance.Length - 1] != ')')
            {
                Error = ("El tipo VARCHAR no está bien definido, asegurese que tenga dos paréntesis al lado del tipo");
                return false;
            }

            try
            {
                varcharlength = VarcharParts[1];
                if (int.Parse(varcharlength) < 0)
                {
                    Error = ("El tipo VARCHAR no está bien definido, asegúrese que tenga un tamaño válido: " + instance);
                    return false;
                }
            }
            catch
            {
                Error = ("El tipo VARCHAR no está bien definido " + instance);
                return false;
            }
            varcharlength = "~" + varcharlength;
            return true;
        }


        private string Traslate(bool ToMain, string word)
        {
            if (!ToMain)
            {
                return ReservatedWords.Find(x => x.Main.Equals(word)).Traduction;
            }
            else
            {
                return ReservatedWords.Find(x => x.Traduction.Equals(word)).Main;
            }
        }
    }
}