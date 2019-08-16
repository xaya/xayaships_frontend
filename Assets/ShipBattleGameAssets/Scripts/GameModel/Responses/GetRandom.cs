using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace BattleShip.BLL.Responses
{
    public class GetRandom
    {
        public static System.Random r = new System.Random();

        public static bool WhoseFirst()
        {            
            return r.Next(1, 10) <= 5;
        }

        public static string GetDirection()
        {
            switch(r.Next(1, 4))
            {
                case 1:
                    return "l";
                case 2:
                    return "r";
                case 3:
                    return "u";
                case 4:
                    return "d";
                default:
                    return "";
            }
        }

        public static int GetLocation()
        {
            return r.Next(1, 10);
        }
        
    }
}
