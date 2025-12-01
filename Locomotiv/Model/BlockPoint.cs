using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Locomotiv.Model.Interfaces;

public class BlockPoint : IMapPoint
{
    public int Id { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public List<Block> Blocks { get; set; } = new List<Block>();
}