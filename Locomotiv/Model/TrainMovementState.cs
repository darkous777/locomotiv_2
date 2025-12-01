using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Model
{
    public class TrainMovementState
    {
        public Train Train { get; set; }
        public PredefinedRoute Route { get; set; }
        public IList<Block> Blocks { get; set; }
        public int CurrentBlockIndex { get; set; } = 0;
        public bool IsMoving { get; set; } = false;
    }
}
