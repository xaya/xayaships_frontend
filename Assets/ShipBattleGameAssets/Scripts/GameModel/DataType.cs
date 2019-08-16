using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SeaBattleControl
{
    public class DataType : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

    }
    //=========== GameBoard Comments =================//
    public class Coordinates
    {
        public int Row { get; set; }
        public int Column { get; set; }

        public Coordinates(int row, int column)
        {
            Row = row;
            Column = column;
        }
    }
    public enum OccupationType
    { Empty, Carrier, Destroyer, Submarine, Patrolboat, Hit, Miss }

    public class Panel
    {
        public OccupationType OccupationType { get; set; }
        public Coordinates Coordinates { get; set; }

        public Panel(int row, int column)
        {
            Coordinates = new Coordinates(row, column);
            OccupationType = OccupationType.Empty;
        }

        public string Status
        {
            get
            {
                return OccupationType.ToString();
            }
        }

        public bool IsOccupied
        {
            get
            {
                return OccupationType == OccupationType.Carrier
                    || OccupationType == OccupationType.Destroyer
                    || OccupationType == OccupationType.Submarine
                    || OccupationType == OccupationType.Patrolboat;
            }
        }

        public bool IsRandomAvailable
        {
            get
            {
                return (Coordinates.Row % 2 == 0 && Coordinates.Column % 2 == 0)
                    || (Coordinates.Row % 2 == 1 && Coordinates.Column % 2 == 1);
            }
        }
    }
    public abstract class Ship
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Hits { get; set; }
        public OccupationType OccupationType { get; set; }
        public bool IsSunk
        {
            get
            {
                return Hits >= Width;
            }
        }
    }

    public class Destroyer : Ship
    {
        public Destroyer()
        {
            Name = "Destroyer";
            Width = 3;
            OccupationType = OccupationType.Destroyer;
        }
    }

    public class Submarine : Ship
    {
        public Submarine()
        {
            Name = "Submarine";
            Width = 3;
            OccupationType = OccupationType.Submarine;
        }
    }

    public class Patrolboat : Ship
    {
        public Patrolboat()
        {
            Name = "Patrolboat";
            Width = 2;
            OccupationType = OccupationType.Patrolboat;
        }
    }

    public class Carrier : Ship
    {
        public Carrier()
        {
            Name = "Aircraft Carrier";
            Width = 4;
            OccupationType = OccupationType.Carrier;
        }
    }
    public class GameBoard
    {
        public List<Panel> Panels { get; set; }

        public GameBoard()
        {
            Panels = new List<Panel>();
            for (int i = 1; i <= 8; i++)
            {
                for (int j = 1; j <= 8; j++)
                {
                    Panels.Add(new Panel(i, j));
                }
            }
        }
        
    }
    public class FiringBoard : GameBoard
    {
        public List<Coordinates> GetOpenRandomPanels() { return null; }

        public List<Coordinates> GetHitNeighbors() { return null; }

        public List<Panel> GetNeighbors(Coordinates coordinates) { return null; }
    }

    public class SeaBattlePlayer
    {
        public string Name { get; set; }
        public GameBoard GameBoard { get; set; }
        public FiringBoard FiringBoard { get; set; }
        public List<Ship> Ships { get; set; }
        public bool HasLost
        {
            get
            {

                foreach (Ship s in Ships)
                {
                    if (s.IsSunk) return true;
                }
                return false;
                //return Ships.All(x => x.IsSunk);
            }
        }

        public SeaBattlePlayer(string name)
        {
            Name = name;
            Ships = new List<Ship>()
        {
            new Carrier(),
            new Destroyer(),
            new Submarine(),
            new Patrolboat(),
            new Patrolboat(),
            new Patrolboat()
        };
            GameBoard = new GameBoard();
            FiringBoard = new FiringBoard();
        }
    }
    public class SeaBattleGame
    {
        public SeaBattlePlayer Player1 { get; set; }
        public SeaBattlePlayer Player2 { get; set; }

        public SeaBattleGame() { }

        public void PlayRound() { }

        public void PlayToEnd() { }
    }
    //public void PlaceShips()
    //{
    //    Random rand = new Random(Guid.NewGuid().GetHashCode());
    //    foreach (var ship in Ships)
    //    {
    //        //Select a random row/column combination, then select a random orientation.
    //        //If none of the proposed panels are occupied, place the ship
    //        //Do this for all ships

    //        bool isOpen = true;
    //        while (isOpen)
    //        {
    //            //Next() has the second parameter be exclusive, while the first parameter is inclusive.
    //            var startcolumn = rand.Next(1, 11);
    //            var startrow = rand.Next(1, 11);
    //            int endrow = startrow, endcolumn = startcolumn;
    //            var orientation = rand.Next(1, 101) % 2; //0 for Horizontal

    //            List<int> panelNumbers = new List<int>();
    //            if (orientation == 0)
    //            {
    //                for (int i = 1; i < ship.Width; i++)
    //                {
    //                    endrow++;
    //                }
    //            }
    //            else
    //            {
    //                for (int i = 1; i < ship.Width; i++)
    //                {
    //                    endcolumn++;
    //                }
    //            }

    //            //We cannot place ships beyond the boundaries of the board
    //            if (endrow > 10 || endcolumn > 10)
    //            {
    //                isOpen = true;
    //                continue; //Restart the while loop to select a new random panel
    //            }

    //            //Check if specified panels are occupied
    //            var affectedPanels = GameBoard.Panels.Range(startrow, startcolumn, endrow, endcolumn);
    //            if (affectedPanels.Any(x => x.IsOccupied))
    //            {
    //                isOpen = true;
    //                continue;
    //            }

    //            foreach (var panel in affectedPanels)
    //            {
    //                panel.OccupationType = ship.OccupationType;
    //            }
    //            isOpen = false;
    //        }
    //    }
    //}
    //var affectedPanels = GameBoard.Panels.Range(startrow, startcolumn, endrow, endcolumn);
    public static class PanelExtensions
    {
        public static List<Panel> Range(this List<Panel> panels, int startRow, int startColumn, int endRow, int endColumn)
        {

            //return panels.Where(x => x.Coordinates.Row >= startRow&& x.Coordinates.Column >= startColumn&& x.Coordinates.Row <= endRow&& x.Coordinates.Column <= endColumn).ToList();
            return null;
        }
    }
}
/*
public void OutputBoards()
{
    Console.WriteLine(Name);
    Console.WriteLine("Own Board:                          Firing Board:");
    for (int row = 1; row <= 10; row++)
    {
        for (int ownColumn = 1; ownColumn <= 10; ownColumn++)
        {
            Console.Write(GameBoard.Panels.At(row, ownColumn).Status + " ");
        }
        Console.Write("                ");
        for (int firingColumn = 1; firingColumn <= 10; firingColumn++)
        {
            Console.Write(FiringBoard.Panels.At(row, firingColumn).Status + " ");
        }
        Console.WriteLine(Environment.NewLine);
    }
    Console.WriteLine(Environment.NewLine);
}
*/