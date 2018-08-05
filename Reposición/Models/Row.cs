using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Reposición.Models
{
    public class Row
    {
        public List<string> Registers = new List<string>();

        public Row(int columns)
        {
            for (int i = 0; i < columns; i++)
            {
                Registers.Add("");
            }
        }
    }
}