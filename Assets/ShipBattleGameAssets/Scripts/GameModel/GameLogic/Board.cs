﻿using System;
using System.Collections.Generic;
using System.Linq;
using BattleShip.BLL.Requests;
using BattleShip.BLL.Responses;
using BattleShip.BLL.Ships;

namespace BattleShip.BLL.GameLogic
{
    public class Board
    {
        public const int xCoordinator = 8;
        public const int yCoordinator = 8;
        private Dictionary<Coordinate, ShotHistory> ShotHistory;
        private int _currentShipIndex;

        public Ship[] Ships { get; private set; }

        public Board()
        {
            ShotHistory = new Dictionary<Coordinate, ShotHistory>();
            Ships = new Ship[7];
            _currentShipIndex = 0;
        }

        public FireShotResponse FireShot(Coordinate coordinate)
        {
            var response = new FireShotResponse();

            // is this coordinate on the board?
            if (!IsValidCoordinate(coordinate))
            {
                response.ShotStatus = ShotStatus.Invalid;
                return response;
            }

            // did they already try this position?
            if (ShotHistory.ContainsKey(coordinate))
            {
                response.ShotStatus = ShotStatus.Duplicate;
                return response;
            }

            CheckShipsForHit(coordinate, response);
            CheckForVictory(response);

            return response;            
        }

        public ShotHistory CheckCoordinate(Coordinate coordinate)
        {
            if(ShotHistory.ContainsKey(coordinate))
            {
                return ShotHistory[coordinate];
            }
            else
            {
                return Responses.ShotHistory.Unknown;
            }
        }

        public ShipPlacement PlaceShip(PlaceShipRequest request,   out Ship newShip)
        {
            newShip = null;
            if (_currentShipIndex > 7)
                throw new Exception("You can not add another ship, 7 is the limit!");

            if (!IsValidCoordinate(request.Coordinate))
                return ShipPlacement.NotEnoughSpace;

            Ship newTempShip = ShipCreator.CreateShip(request.ShipType,this);

            ShipPlacement Placement= newTempShip.SetShipPositions(request.Coordinate);
            if(Placement==ShipPlacement.Ok)
            {
                AddShipToBoard(newTempShip);
                newShip = newTempShip;
            }
            return Placement;
           
        }

        public int GetCurrnetShipIndex()
        {
            return _currentShipIndex;
        }
        private void CheckForVictory(FireShotResponse response)
        {
            if (response.ShotStatus == ShotStatus.HitAndSunk)
            {
                // did they win?
                if (Ships.All(s => s.IsSunk))
                    response.ShotStatus = ShotStatus.Victory;
            }
        }

        private void CheckShipsForHit(Coordinate coordinate, FireShotResponse response)
        {
            response.ShotStatus = ShotStatus.Miss;

            foreach (var ship in Ships)
            {
                // no need to check sunk ships
                if (ship.IsSunk)
                    continue;

                ShotStatus status = ship.FireAtShip(coordinate);

                switch (status)
                {
                    case ShotStatus.HitAndSunk:
                        response.ShotStatus = ShotStatus.HitAndSunk;
                        response.ShipImpacted = ship.ShipName;
                        ShotHistory.Add(coordinate, Responses.ShotHistory.Hit);
                        break;
                    case ShotStatus.Hit:
                        response.ShotStatus = ShotStatus.Hit;
                        response.ShipImpacted = ship.ShipName;
                        ShotHistory.Add(coordinate, Responses.ShotHistory.Hit);
                        break;
                }

                // if they hit something, no need to continue looping
                if (status != ShotStatus.Miss)
                    break;
            }

            if (response.ShotStatus == ShotStatus.Miss)
            {
                ShotHistory.Add(coordinate, Responses.ShotHistory.Miss);
            }
        }

        public bool IsValidCoordinate(Coordinate coordinate)
        {
            if (coordinate == null) return false;

            return coordinate.XCoordinate >= 1 && coordinate.XCoordinate <= xCoordinator &&
            coordinate.YCoordinate >= 1 && coordinate.YCoordinate <= yCoordinator;
        }

        private void AddShipToBoard(Ship newShip)
        {
            Ships[_currentShipIndex] = newShip;
            _currentShipIndex++;
        }

        public bool OverlapsAnotherShip(Coordinate coordinate, Ship ownShip=null)
        {
            foreach (var ship in Ships)
            {
                if (ship != null)
                {
                    if (ship == ownShip) continue;
                    if (ship.BoardPositions.Contains(coordinate))
                        return true;
                }
            }
            return false;
        }

        public bool ValidatePositions()
        {
            bool bRet = true;
            int[][] matrixShipsIndex = new int[8][];
            for (int i = 0; i < 8; i++)
            {
                matrixShipsIndex[i] = new int[8];
            }

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    matrixShipsIndex[i][j] = -1;
                    for (int sIndex = 0; sIndex < Ships.Length; sIndex++)
                    {
                        if (Ships[sIndex].BoardPositions.Contains(new Coordinate(i + 1, j + 1)))
                        {
                            matrixShipsIndex[i][j] = sIndex;
                        }
                    }
                }
            }

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (matrixShipsIndex[i][j] >= 0)
                    {
                        //near other ships exist?
                        for (int ii = i - 1; ii <= i + 1; ii++)
                        {
                            for (int jj = j - 1; jj <= j + 1; jj++)
                            {
                                if (ii < 0 || jj < 0 || ii > 7 || jj > 7) continue;
                                if (matrixShipsIndex[ii][jj] >= 0 && matrixShipsIndex[ii][jj] != matrixShipsIndex[i][j])
                                {
                                    bRet = false;
                                }
                            }
                        }
                    }
                }
            }

            return bRet;
        }

        public int CountOfShips()
        {
            int count = 0;
            foreach(Ship s in Ships)
            {
                if (s != null) count++;
            }
            return count;
        }
    }
}
