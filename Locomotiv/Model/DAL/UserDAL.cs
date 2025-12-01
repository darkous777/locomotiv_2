using System.Collections.Generic;
using System.Linq;
using Locomotiv.Model.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Locomotiv.Model.DAL
{
    public class UserDAL : IUserDAL
    {
        private readonly ApplicationDbContext _context;

        public UserDAL(ApplicationDbContext _db)
        {
            _context = _db;
        }

        public User? FindByUsernameAndPassword(string u, string p)
        {
            return _context.Users
                .Include(user => user.Station)
                    .ThenInclude(s => s.Trains)
                        .ThenInclude(t => t.Locomotives)
                .Include(user => user.Station)
                    .ThenInclude(s => s.Trains)
                        .ThenInclude(t => t.Wagons)
                .Include(user => user.Station)
                    .ThenInclude(s => s.TrainsInStation)
                        .ThenInclude(t => t.Locomotives)
                .Include(user => user.Station)
                    .ThenInclude(s => s.TrainsInStation)
                        .ThenInclude(t => t.Wagons)
                .FirstOrDefault(u2 => u2.Username == u && u2.Password == p);
        }
    }
}
