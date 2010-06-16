using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BubbleMatch
{
    public class Arithmetic
    {
        public static string operators = "+-*/()";
        public Random rand = new Random();
        public Arithmetic()
        {

        }

        public string createNewForm(int canonicalNum,int difficulty)
        {
            double d = rand.NextDouble();
            if (difficulty == 1)
            {
                if (d<.25)// add
                {
                    int x = (int)Math.Ceiling(rand.NextDouble()*canonicalNum);
                    return x + "+" + (canonicalNum - x);
                }
                else if (d<.5)
                {
                    int x = (int)Math.Ceiling(canonicalNum + rand.NextDouble() * 10);
                    return x + "-" + (x-canonicalNum);
                }
                else if (d < .75)
                {
                    int x = getFactor(canonicalNum);
                    return x + "*" + (canonicalNum/x);
                }
                else
                {
                    int x = (int)Math.Ceiling(rand.NextDouble()*10);
                    
                    return x*canonicalNum + "/" + x;
                }
                
                
            }
            
            return "";
        }

        public int getFactor(int x)
        {
            List<int> factors = new List<int>();
            for (int i = 1; i <= Math.Sqrt(x); i++)
            {
                if (x % i == 0)
                {
                    factors.Add(i);
                }
            }

            return factors[(int)(Math.Floor(rand.NextDouble() * factors.Count()))];
        }
       
          

    }
}
