using System.Linq;
using BattleShip.BLL.Requests;
using BattleShip.BLL.Responses;
using BattleShip.BLL.GameLogic;

namespace BattleShip.BLL.Ships
{
    public class Ship
    {

        
        public ShipType ShipType { get; private set; }
        public string ShipName { get { return ShipType.ToString(); } }
        public Coordinate[] BoardPositions { get; set; }
        //==================== add ===========//
        public Board board;  
        public Coordinate curCoodinate { get; set; }
        public ShipDirection direction { get; set; }
        public int Width { get; set; }
        //=====================================//
        private int _lifeRemaining;
        public bool IsSunk { get { return _lifeRemaining == 0; } }

        public Ship(ShipType shipType, int numberOfSlots, Board board1)
        {
            ShipType = shipType;
            _lifeRemaining = numberOfSlots;
            board = board1;
            Width = numberOfSlots;
            BoardPositions = new Coordinate[numberOfSlots];
            direction = ShipDirection.Left;
        }

        public ShotStatus FireAtShip(Coordinate position)
        {
            if (BoardPositions.Contains(position))
            {
                _lifeRemaining--;

                if(_lifeRemaining == 0)
                    return ShotStatus.HitAndSunk;

                return ShotStatus.Hit;
            }

            return ShotStatus.Miss;
        }      
        
        public ShipPlacement SetShipPositions(Coordinate coordinate, ShipDirection tempShipDirection=ShipDirection.Left)
        {
            Coordinate[] positions = new Coordinate[Width];
            
            int i;
            int positionIndex = 0;
            switch (tempShipDirection)
            {
                case ShipDirection.Left:
                    for (i = coordinate.XCoordinate; i > coordinate.XCoordinate-Width; i--)
                    {                     
                        positions[positionIndex] = new Coordinate(i, coordinate.YCoordinate);
                        positionIndex++;
                    }
                    break;
                case ShipDirection.Right:
                    for (i = coordinate.XCoordinate; i < coordinate.XCoordinate + Width; i++)
                    {
                        positions[positionIndex] = new Coordinate(i, coordinate.YCoordinate);
                        positionIndex++;
                    }
                    break;
                case ShipDirection.Up:
                    for (i = coordinate.YCoordinate; i > coordinate.YCoordinate - Width; i--)
                    {
                        positions[positionIndex] = new Coordinate(coordinate.XCoordinate, i);
                        positionIndex++;
                    }
                    break;
                case ShipDirection.Down:
                    for (i = coordinate.YCoordinate; i < coordinate.YCoordinate + Width; i++)
                    {
                        positions[positionIndex] = new Coordinate(coordinate.XCoordinate, i);
                        positionIndex++;
                    }
                    break;
            }
            foreach(Coordinate coordinateInShip in positions)
            {

                if (!board.IsValidCoordinate(coordinateInShip))
                    return ShipPlacement.NotEnoughSpace;

                if (board.OverlapsAnotherShip(coordinateInShip, this))
                    return ShipPlacement.Overlap;
            }
            curCoodinate = coordinate;
            direction = tempShipDirection;
            BoardPositions = positions;
            return ShipPlacement.Ok;
        }

        //============== Don't change Direction of Ship ==============//
        public ShipPlacement SetShipPositionsOnly(Coordinate coordinate)
        {
            Coordinate[] positions = new Coordinate[Width];

            int i;
            int positionIndex = 0;
            switch (direction)
            {
                case ShipDirection.Left:
                    for (i = coordinate.XCoordinate; i > coordinate.XCoordinate - Width; i--)
                    {
                        positions[positionIndex] = new Coordinate(i, coordinate.YCoordinate);
                        positionIndex++;
                    }
                    break;
                case ShipDirection.Right:
                    for (i = coordinate.XCoordinate; i < coordinate.XCoordinate + Width; i++)
                    {
                        positions[positionIndex] = new Coordinate(i, coordinate.YCoordinate);
                        positionIndex++;
                    }
                    break;
                case ShipDirection.Up:
                    for (i = coordinate.YCoordinate; i > coordinate.YCoordinate - Width; i--)
                    {
                        positions[positionIndex] = new Coordinate(coordinate.XCoordinate, i);
                        positionIndex++;
                    }
                    break;
                case ShipDirection.Down:
                    for (i = coordinate.YCoordinate; i < coordinate.YCoordinate + Width; i++)
                    {
                        positions[positionIndex] = new Coordinate(coordinate.XCoordinate, i);
                        positionIndex++;
                    }
                    break;
            }
            foreach (Coordinate coordinateInShip in positions)
            {

                if (!board.IsValidCoordinate(coordinateInShip))
                    return ShipPlacement.NotEnoughSpace;

                if (board.OverlapsAnotherShip(coordinateInShip, this))
                    return ShipPlacement.Overlap;
            }
            curCoodinate = coordinate;            
            BoardPositions = positions;
            return ShipPlacement.Ok;
        }

        //=================== Don't change Position of Ship=========================================//
        public ShipPlacement SwapDirectionOnly()
        {
            Coordinate coordinate = curCoodinate;
            Coordinate[] positions = new Coordinate[Width];

            int i;
            int positionIndex = 0;
            ShipDirection tempShipDirection= (ShipDirection) (((int)direction+1)%4);

            switch (tempShipDirection)
            {
                case ShipDirection.Left:
                    for (i = coordinate.XCoordinate; i > coordinate.XCoordinate - Width; i--)
                    {
                        positions[positionIndex] = new Coordinate(i, coordinate.YCoordinate);
                        positionIndex++;
                    }
                    break;
                case ShipDirection.Right:
                    for (i = coordinate.XCoordinate; i < coordinate.XCoordinate + Width; i++)
                    {
                        positions[positionIndex] = new Coordinate(i, coordinate.YCoordinate);
                        positionIndex++;
                    }
                    break;
                case ShipDirection.Up:
                    for (i = coordinate.YCoordinate; i > coordinate.YCoordinate - Width; i--)
                    {
                        positions[positionIndex] = new Coordinate(coordinate.XCoordinate, i);
                        positionIndex++;
                    }
                    break;
                case ShipDirection.Down:
                    for (i = coordinate.YCoordinate; i < coordinate.YCoordinate + Width; i++)
                    {
                        positions[positionIndex] = new Coordinate(coordinate.XCoordinate, i);
                        positionIndex++;
                    }
                    break;
            }
            foreach (Coordinate coordinateInShip in positions)
            {

                if (!board.IsValidCoordinate(coordinateInShip))
                    return ShipPlacement.NotEnoughSpace;

                if (board.OverlapsAnotherShip(coordinateInShip, this))
                    return ShipPlacement.Overlap;
            }
            //curCoodinate = coordinate;
            direction = tempShipDirection;
            BoardPositions = positions;
            return ShipPlacement.Ok;
        }
    }
}
