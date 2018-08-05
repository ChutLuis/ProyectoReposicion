using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Reposición.Models
{
    public class Path_
    {
        private static Path_ instance;

        public static Path_ Instance
        {
            get
            {
                if (instance == null) return instance = new Path_();

                return instance;
            }
        }

        public string a1;

        public Path_()
        {
            a1 = "";
        }

    }
}