using Locomotiv.Model.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Model.DAL
{
    public class BlockDAL : IBlockDAL
    {
        private readonly ApplicationDbContext _context;

        public BlockDAL(ApplicationDbContext c)
        {
            _context = c;
        }

        public IList<Block> GetAll()
        {
            return _context.Blocks
                .Include(b => b.Points)
                .Include(b => b.CurrentTrain)
                .ToList();
        }

        public IList<Train> GetTrainsCurrentlyOnBlocks()
        {
            return _context.Blocks
                .Where(b => b.CurrentTrain != null)
                .Select(b => b.CurrentTrain)
                .ToList();
        }

        public void Update(Block block)
        {
            _context.Blocks.Update(block);
            _context.SaveChanges();
        }


        public IList<Block> GetBlocksByPointId(int blockPointId)
        {
            return _context.Blocks
                .Include(b => b.Points)
                .Include(b => b.CurrentTrain)
                .Where(b => b.Points.Any(p => p.Id == blockPointId))
                .ToList();
        }
    }
}
