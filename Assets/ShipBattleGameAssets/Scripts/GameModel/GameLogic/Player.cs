using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using BattleShip.BLL.GameLogic;
using BattleShip.BLL.Requests;
using BattleShip.BLL.Responses;
using BattleShip.BLL.Ships;

namespace BattleShip.UI
{
    public class Player
    {
        public string Name{ get; set;}
        public int Win { get; set; }
        public bool IsPC { get; set; }
        public Board PlayerBoard;
        public GameLevel GameLevel { get; set; }
    }
}
