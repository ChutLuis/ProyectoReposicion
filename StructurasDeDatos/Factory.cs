using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StructurasDeDatos
{
    class Factory
    {
        public static int makeNull()
        {
            return int.MinValue;
        }

        public static int CountCharactersOfANull()
        {
            return makeNull().ToString().Length;
        }

        public static string makeHeader(string directionRoot, string directionLast, int TreeOrder, int height)
        {
            string header = "";
            header += directionRoot + Environment.NewLine;
            header += directionLast + Environment.NewLine;
            header += Factory.FixPositionsSize(Math.Abs(TreeOrder - 1)) + Environment.NewLine;
            header += FixPositionsSize(TreeOrder) + Environment.NewLine;
            header += FixPositionsSize(height) + Environment.NewLine;

            return header;
        }

        public static string FixPositionsSize(int position)
        {
            string sNull = Factory.makeNull().ToString();

            if (position.ToString().Length == sNull.Length)
            {
                return position.ToString();
            }
            else
            {
                string NewPosition = position.ToString();
                for (int i = position.ToString().Length; i < sNull.Length; i++)
                {
                    NewPosition = "0" + NewPosition; // Dude to positions are always intergers values, fix them with 0's it's ok.
                }
                return NewPosition;
            }
        }

        public static string FixDataSize(string data, int MaxLengthData)
        {
            if (data.Length == MaxLengthData)
            {
                return data;
            }
            else
            {
                string NewData = data;
                for (int i = data.ToString().Length; i < MaxLengthData; i++)
                {
                    NewData = "~" + NewData;
                }
                return NewData;
            }
        }

        public static string FixKeySize(string key, int MaxLengthKey)
        {
            if (key.Length == MaxLengthKey)
            {
                return key;
            }
            else
            {
                string NewData = key;
                for (int i = key.ToString().Length; i < MaxLengthKey; i++)
                {
                    NewData = "~" + NewData;
                }
                return NewData;
            }
        }

        public static string ReturnOriginalData(string data)
        {
            string newData = "";
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].ToString() != "~")
                {
                    newData += data[i].ToString();
                }
            }
            return newData;
        }

        public static string ReturnOriginalKey(string data)
        {
            string newData = "";
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].ToString() != "~")
                {
                    newData += data[i].ToString();
                }
            }
            return newData;
        }

        public static string MakeNullData(int MaxlengthData)
        {
            string nulldata = "";
            for (int i = 0; i < MaxlengthData; i++)
            {
                nulldata += "~";
            }
            return nulldata;
        }

        public static string MakeNullKey(int MaxlengthKey)
        {
            string nullKey = "";
            for (int i = 0; i < MaxlengthKey; i++)
            {
                nullKey += "~";
            }
            return nullKey;
        }

        public static string CleanDataFix(string value)
        {
            if (value == null)
            {
                return null;
            }
            return value.Substring(value.LastIndexOf("0") + 1);
        }


    }
}

