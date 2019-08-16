using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using BattleShip.BLL.Ships;

namespace BattleShip.BLL.GameLogic
{
    public class ShipCreator
    {
        public static Ship CreateShip(ShipType type, Board board)
        {
            switch (type)
            {
                case ShipType.Destroyer:
                    return new Ship(ShipType.Destroyer, 3, board);
                case ShipType.Patrolboat:
                    return new Ship(ShipType.Patrolboat, 2,board);
                case ShipType.Submarine:
                    return new Ship(ShipType.Submarine, 3,board);               
                default:
                    return new Ship(ShipType.Carrier, 4,board);
            }
        }
    }
}
