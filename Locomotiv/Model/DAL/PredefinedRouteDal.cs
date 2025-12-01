using Locomotiv.Model.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Model.DAL
{
    public class PredefinedRouteDal : IPredefinedRouteDAL
    {
        private readonly ApplicationDbContext _context;

        public PredefinedRouteDal(ApplicationDbContext _db)
        {
            _context = _db;
        }

        public IList<PredefinedRoute> GetAll()
        {
            return _context.PredefinedRoutes
                .Include(b => b.StartStation)
                .Include(b => b.EndStation)
                .ToList();
        }

        public PredefinedRoute? GetByStations(Station start, Station end)
        {
            return _context.PredefinedRoutes
                .Include(r => r.StartStation)
                .Include(r => r.EndStation)
                .FirstOrDefault(r =>
                    r.StartStation.Id == start.Id &&
                    r.EndStation.Id == end.Id
                );
        }
    }
}
