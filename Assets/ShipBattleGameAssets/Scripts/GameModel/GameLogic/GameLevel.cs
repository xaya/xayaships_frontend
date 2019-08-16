using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace BattleShip.BLL.GameLogic
{
    public enum GameLevel
    {
        Easy,
        Medium,
        Hard
    }
    public enum GameStatus
    {
        None, Ready, Playing, End
    }
    public class GameControl
    {
        public GameStatus currentStatus;
        public BattleShip.BLL.GameLogic.Board gameMyBoard;
        public BattleShip.BLL.GameLogic.Board gameEnemyBoard;

        public GameControl()
        {
            currentStatus = GameStatus.Ready;
            gameMyBoard = new BattleShip.BLL.GameLogic.Board();
            gameEnemyBoard = new BattleShip.BLL.GameLogic.Board();
        }
        public void SetupBoard()
        {
            
        }
    }
}
