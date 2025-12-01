using System;
using System.Collections.Generic;
using System.Linq;
using Locomotiv.Model.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Locomotiv.Model.DAL
{
    public class StationDAL : IStationDAL
    {
        private readonly ApplicationDbContext _context;

        public StationDAL(ApplicationDbContext _db)
        {
            _context = _db;
        }

        public Station? FindById(int id)
        {
          return _context.Stations
              .Include(s => s.Trains)
                  .ThenInclude(t => t.Locomotives)
              .Include(s => s.Trains)
                  .ThenInclude(t => t.Wagons)
              .Include(s => s.Trains)
                  .ThenInclude(t => t.PredefinedRoute)
              .Include(s => s.TrainsInStation)
                  .ThenInclude(t => t.Locomotives)
              .Include(s => s.TrainsInStation)
                  .ThenInclude(t => t.Wagons)
              .Include(s => s.TrainsInStation)
                  .ThenInclude(t => t.PredefinedRoute)
              .FirstOrDefault(s => s.Id == id);
        }

        public IList<Station> GetAll()
        {
          return _context.Stations
              .Include(s => s.Trains)
                  .ThenInclude(t => t.Locomotives)
              .Include(s => s.Trains)
                  .ThenInclude(t => t.Wagons)
              .Include(s => s.Trains)
                  .ThenInclude(t => t.PredefinedRoute)
              .Include(s => s.TrainsInStation)
                  .ThenInclude(t => t.Locomotives)
              .Include(s => s.TrainsInStation)
                  .ThenInclude(t => t.Wagons)
              .Include(s => s.TrainsInStation)
                  .ThenInclude(t => t.PredefinedRoute)
              .ToList();
        }
        public IList<Train> GetAllTrain()
        {
            return _context.Trains
                .Include(t => t.Locomotives)
                .Include(t => t.Wagons)
                .Include(t => t.PredefinedRoute)
                    .ThenInclude(r => r.StartStation)
                .Include(t => t.PredefinedRoute)
                    .ThenInclude(r => r.EndStation)
                .ToList();
        }

        public IList<Train> GetTrainsForStation(int stationId)
        {
            Station? station = _context.Stations
                .Include(s => s.Trains)
                    .ThenInclude(t => t.Locomotives)
                .Include(s => s.Trains)
                    .ThenInclude(t => t.Wagons)
                .Include(s => s.Trains)
                    .ThenInclude(t => t.PredefinedRoute)
                .FirstOrDefault(s => s.Id == stationId);

            return station?.Trains?.ToList() ?? new List<Train>();
        }

        public IList<Train> GetTrainsInStation(int stationId)
        {
            Station? station = _context.Stations
                .Include(s => s.TrainsInStation)
                    .ThenInclude(t => t.Locomotives)
                .Include(s => s.TrainsInStation)
                    .ThenInclude(t => t.Wagons)
                .Include(s => s.TrainsInStation)
                    .ThenInclude(t => t.PredefinedRoute)
                .FirstOrDefault(s => s.Id == stationId);

            return station?.TrainsInStation?.ToList() ?? new List<Train>();
        }

        public void RemoveTrainFromAllStations(int trainId)
        {
            IList<Station> allStations = _context.Stations
                .Include(s => s.Trains)
                .Include(s => s.TrainsInStation)
                .ToList();

            Train? train = _context.Trains.Find(trainId);
            if (train == null)
                return;

            foreach (Station station in allStations)
            {
                bool modified = false;

                if (station.Trains != null && station.Trains.Contains(train))
                {
                    station.Trains.Remove(train);
                    modified = true;
                }

                if (station.TrainsInStation != null && station.TrainsInStation.Contains(train))
                {
                    station.TrainsInStation.Remove(train);
                    modified = true;
                }
            }

            _context.SaveChanges();
        }

        public void RemoveTrainFromStation(int stationId, int trainId)
        {
            Station? station = _context.Stations
                .Include(s => s.Trains)
                .Include(s => s.TrainsInStation)
                .FirstOrDefault(s => s.Id == stationId);

            if (station != null)
            {
                Train? trainToRemove = _context.Trains.Find(trainId);
                if (trainToRemove != null)
                {
                    if (station.Trains != null && station.Trains.Contains(trainToRemove))
                    {
                        station.Trains.Remove(trainToRemove);
                    }

                    if (station.TrainsInStation != null && station.TrainsInStation.Contains(trainToRemove))
                    {
                        station.TrainsInStation.Remove(trainToRemove);

                        if (station.Trains == null)
                        {
                            station.Trains = new List<Train>();
                        }
                        if (!station.Trains.Contains(trainToRemove))
                        {
                            station.Trains.Add(trainToRemove);
                        }
                    }

                    _context.SaveChanges();
                }
            }
        }

        public void AddTrainToStation(int stationId, int trainId, bool addToTrainsInStation)
        {
            Station? station = _context.Stations
                .Include(s => s.Trains)
                .Include(s => s.TrainsInStation)
                .FirstOrDefault(s => s.Id == stationId);

            if (station != null)
            {
                Train? train = _context.Trains.Find(trainId);
                if (train != null)
                {
                    if (addToTrainsInStation)
                    {
                        if (station.TrainsInStation == null)
                        {
                            station.TrainsInStation = new List<Train>();
                        }

                        int currentCount = station.TrainsInStation.Count;
                        bool hasCapacity = currentCount < station.Capacity;

                        if (hasCapacity)
                        {
                            if (!station.TrainsInStation.Contains(train))
                            {
                                station.TrainsInStation.Add(train);
                            }

                            if (station.Trains != null && station.Trains.Contains(train))
                            {
                                station.Trains.Remove(train);
                            }
                        }
                        else
                        {
                            if (station.Trains == null)
                            {
                                station.Trains = new List<Train>();
                            }
                            if (!station.Trains.Contains(train))
                            {
                                station.Trains.Add(train);
                            }
                        }
                    }
                    else
                    {
                        if (station.Trains == null)
                        {
                            station.Trains = new List<Train>();
                        }
                        if (!station.Trains.Contains(train))
                        {
                            station.Trains.Add(train);
                        }
                    }

                    _context.SaveChanges();
                }
            }
        }

        public void CreateTrainForStation(int stationId, Train train)
        {
            Station? station = _context.Stations
                .Include(s => s.Trains)
                .FirstOrDefault(s => s.Id == stationId);
            if (station != null)
            {
                if (station.Trains == null)
                {
                    station.Trains = new List<Train>();
                }

                _context.Trains.Add(train);
                station.Trains.Add(train);
                _context.SaveChanges();
            }
        }

        public void DeleteTrainPermanently(int stationId, int trainId)
        {
            Station? station = _context.Stations
                .Include(s => s.Trains)
                .FirstOrDefault(s => s.Id == stationId);

            Train? train = _context.Trains
                .Include(t => t.Locomotives)
                .Include(t => t.Wagons)
                .FirstOrDefault(t => t.Id == trainId);

            if (station != null && train != null)
            {
                if (station.Trains != null && station.Trains.Contains(train))
                {
                    station.Trains.Remove(train);
                }

                if (train.Locomotives != null)
                {
                    _context.Locomotives.RemoveRange(train.Locomotives);
                }

                if (train.Wagons != null)
                {
                    _context.Wagons.RemoveRange(train.Wagons);
                }

                _context.Trains.Remove(train);
                _context.SaveChanges();
            }
        }

        public IList<PredefinedRoute> PrefefinedRouteForEachTrain()
        {
            IList<PredefinedRoute> routes = _context.PredefinedRoutes
                .Include(s => s.StartStation)
                .Include(s => s.EndStation)
                .ToList();


            return routes;
        }

        public IList<Block> GetBlocksForPredefinedRoute(List<int> idBlocks) 
        {
            IList<Block> blocks = new List<Block>();

            foreach (int id in idBlocks)
            {
                blocks.Add(_context.Blocks
                    .Include(b => b.Points)
                    .Include(b => b.CurrentTrain)
                    .FirstOrDefault(b => b.Id == id)!);
            }

            return blocks;
        }
    }
}