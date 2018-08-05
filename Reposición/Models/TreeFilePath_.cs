using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Reposición.Models
{
    public class TreeFilePath_
    {
        private static TreeFilePath_ instance;

        public static TreeFilePath_ Instance
        {
            get
            {
                if (instance == null) return instance = new TreeFilePath_();

                return instance;
            }
        }

        public string a1;

        public TreeFilePath_()
        {
            a1 = "";
        }

    }
}