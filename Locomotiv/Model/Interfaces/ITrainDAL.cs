using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Model.Interfaces
{
    public interface ITrainDAL
    {
        IList<Train> GetAll();
        Train? GetById(int id);
        void Add(Train train);
        void Update(Train train);
        void Delete(int id);
    }
}
