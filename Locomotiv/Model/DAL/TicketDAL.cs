using Locomotiv.Model.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Locomotiv.Model.DAL
{
    public class TicketDAL : ITicketDAL
    {
        private readonly ApplicationDbContext _context;

        public TicketDAL(ApplicationDbContext _db)
        {
            _context = _db;
        }

        public Ticket? GetById(int id)
        {
            return _context.Tickets
                .Include(t => t.Train)
                    .ThenInclude(train => train.PredefinedRoute)
                .Include(t => t.User)
                .FirstOrDefault(t => t.Id == id);
        }

        public IList<Ticket> GetAll()
        {
            return _context.Tickets
                .Include(t => t.Train)
                    .ThenInclude(train => train.PredefinedRoute)
                .Include(t => t.User)
                .ToList();
        }

        public void Add(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
            _context.SaveChanges();
        }

        public void Update(Ticket ticket)
        {
            _context.Tickets.Update(ticket);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var ticket = GetById(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
                _context.SaveChanges();
            }
        }

        public IList<Ticket> GetTicketsByUser(int userId)
        {
            return _context.Tickets
                .Include(t => t.Train)
                    .ThenInclude(train => train.PredefinedRoute)
                .Where(t => t.User.Id == userId)
                .ToList();
        }

        public IList<Ticket> GetTicketsByTrain(int trainId)
        {
            return _context.Tickets
                .Include(t => t.User)
                .Where(t => t.Train.Id == trainId)
                .ToList();
        }
    }
}
