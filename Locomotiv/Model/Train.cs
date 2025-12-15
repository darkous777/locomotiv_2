using Locomotiv.Model;
using Locomotiv.Model.Interfaces;
using System;
using System.ComponentModel;
using System.Security.Cryptography;

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

    public ICollection<Ticket> Tickets { get; set; }

    public int Capacity
    {
        get
        {
            if (Locomotives == null)
                return 0;

            int totalCapacity = 0;

            foreach (Locomotive locomotive in Locomotives)
            {
                totalCapacity += locomotive.PassengerCapacity;
            }

            return totalCapacity;
        }
    }

    public int ReservedSeats => Tickets?.Count ?? 0;

    public int AvailableSeats => Capacity - ReservedSeats;

    public bool IsFull => AvailableSeats <= 0;

    public bool IsAlmostFull => AvailableSeats > 0 && AvailableSeats <= 5;


    public override string ToString()
    {
        return $"{TypeOfTrain} (Train #{Id})";
    }
}
