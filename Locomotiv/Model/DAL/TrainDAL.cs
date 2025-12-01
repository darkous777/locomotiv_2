using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Locomotiv.Model.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace Locomotiv.Model.DAL
{
    public class TrainDAL : ITrainDAL
    {
        private readonly ApplicationDbContext _context;

        public TrainDAL(ApplicationDbContext _db)
        {
            _context = _db;
        }

        public IList<Train> GetAll()
        {
            return _context.Trains
                .Include(t => t.Locomotives)
                .Include(t => t.Wagons)
                .ToList();
        }

        public Train? GetById(int id)
        {
            return _context.Trains
                .Include(t => t.Locomotives)
                .Include(t => t.Wagons)
                .FirstOrDefault(t => t.Id == id);
        }

        public void Add(Train train)
        {
            _context.Trains.Add(train);
            _context.SaveChanges();
        }

        public void Update(Train train)
        {
            _context.Trains.Update(train);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            Train train = _context.Trains.Find(id);
            if (train != null)
            {
                _context.Trains.Remove(train);
                _context.SaveChanges();
            }
        }

        public void UpdatePredefinedRoute(PredefinedRoute predefinedRoute)
        {
            _context.PredefinedRoutes.Update(predefinedRoute);
            _context.SaveChanges();
        }
    }
}