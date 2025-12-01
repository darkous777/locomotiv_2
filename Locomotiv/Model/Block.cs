using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Locomotiv.Model.Interfaces;

public class Block : IMapPoint
{
    public int Id { get; set; }

    public List<BlockPoint> Points { get; set; } = new List<BlockPoint>();

    public Train? CurrentTrain { get; set; }

    public double Longitude { get; set; }

    public double Latitude { get; set; }
}
    