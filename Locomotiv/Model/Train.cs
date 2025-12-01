using System;
using System.Security.Cryptography;
using Locomotiv.Model;
using Locomotiv.Model.Interfaces;

public class Train : IMapPoint
{
    public int Id { get; set; }

    public TrainType TypeOfTrain { get; set; }

    public PriorityLevel PriotityLevel { get; set; }

    public TrainState State { get; set; }

    public ICollection<Wagon> Wagons { get; set; }


    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public ICollection<Locomotive> Locomotives { get; set; }

    public PredefinedRoute? PredefinedRoute { get; set; }

    public override string ToString()
    {
        return $"{TypeOfTrain} (Train #{Id})";
    }
}
