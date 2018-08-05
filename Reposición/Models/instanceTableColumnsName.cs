using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Reposición.Models
{
    public class instanceTableColumnsName
    {
        private static instanceTableColumnsName instance;

        public static instanceTableColumnsName Instance
        {
            get
            {
                if (instance == null) return instance = new instanceTableColumnsName();

                return instance;
            }
        }

        public List<string> a1;

        public instanceTableColumnsName()
        {
            a1 = new List<string>();
        }
    }
}