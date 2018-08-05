using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Reposición.Models
{
    public class Instructions
    {
        public string Main { get; private set; }
        public string Traduction { get; private set; }

        public Instructions(string Main, string Traduction)
        {
            this.Main = Main.ToUpper();
            this.Traduction = Traduction.ToUpper();
        }

        public string TraslateToMain(string WordOtherLanguage)
        {
            WordOtherLanguage = WordOtherLanguage.ToUpper();
            if (WordOtherLanguage != Traduction)
            {
                return "!";
            }
            else
            {
                return Main;
            }
        }

        public string TraslateToOther(string instruction)
        {
            instruction = instruction.ToUpper();
            if (instruction != Traduction)
            {
                return "!";
            }
            else
            {
                return Traduction;
            }
        }

        public float EqualityPorcentage(string InstructionToComparate)
        {
            string more = Traduction;
            string less = InstructionToComparate;
            int intResult = 0;

            if (InstructionToComparate.Length > Traduction.Length)
            {
                more = InstructionToComparate;
                less = Traduction;
            }

            for (int i = 0; i < less.Length; i++)
            {
                if (more[i] == less[i])
                {
                    intResult++;
                }
            }

            return intResult / less.Length;
        }

    }
}
