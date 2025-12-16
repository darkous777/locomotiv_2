using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Model
{
    public class Ticket
    {
        public int Id { get; set; }

        public Train Train { get; set; }

        public User User { get; set; }

    }
}
