using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Reposición.Models
{
    public class TableFilePath_
    {
        private static TableFilePath_ instance;

        public static TableFilePath_ Instance
        {
            get
            {
                if (instance == null) return instance = new TableFilePath_();

                return instance;
            }
        }

        public string a1;

        public TableFilePath_()
        {
            a1 = "";
        }

    }
}