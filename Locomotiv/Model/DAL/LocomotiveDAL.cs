using Locomotiv.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Model.DAL
{
    public class LocomotiveDAL : ILocomotiveDAL
    {
        private readonly ApplicationDbContext _context;

        public LocomotiveDAL(ApplicationDbContext _db)
        {
            _context = _db;
        }

        public IList<Locomotive> GetAll()
        {
            return _context.Locomotives.ToList();
        }
    }
}
