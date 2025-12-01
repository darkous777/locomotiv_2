using Locomotiv.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Model.DAL
{
    public class BlockPointDAL : IBlockPointDAL
    {
        private readonly ApplicationDbContext _context;

        public BlockPointDAL(ApplicationDbContext _db)
        {
            _context = _db;
        }

        public IList<BlockPoint> GetAll()
        {
            return _context.BlockPoints.ToList();
        }
    }
}
