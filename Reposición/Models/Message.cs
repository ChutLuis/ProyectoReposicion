    using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Reposición.Models
{
    public class Message
    {
        private static Message instance;

        public static Message Instance
        {
            get
            {
                if (instance == null) return instance = new Message();

                return instance;
            }
        }

        public string a1;

        public Message()
        {
            a1 = "";
        }

    }
}