using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Model.DAL
{
    internal class TicketDAL
    {
        private readonly ApplicationDbContext _context;

        public TicketDAL(ApplicationDbContext _db)
        {
            _context = _db;
        }
        public Ticket? FindById(int id)
        {
            return _context.Tickets
                .FirstOrDefault(s => s.Id == id);
        }

        public IList<Ticket> GetAll()
        {
            return _context.Tickets.ToList();
        }
    }
}
