using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Reposición.Models
{
    public class Column
    {
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public int MaxLength { get; set; }
        public bool IsPrimaryKey { get; set; }

        public Column() { }

        public Column(string columnName, string type, int maxLength, bool isPrimaryKey)
        {
            ColumnName = columnName;
            DataType = type;
            MaxLength = maxLength;
            IsPrimaryKey = isPrimaryKey;
        }
    }
}