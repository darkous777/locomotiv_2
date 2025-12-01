using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Model.Interfaces
{
    public interface IBlockDAL
    {
        IList<Block> GetAll();
        IList<Train> GetTrainsCurrentlyOnBlocks();
        void Update(Block block);
        IList<Block> GetBlocksByPointId(int blockPointId);
    }
}
