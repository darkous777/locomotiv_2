namespace Locomotiv.Model.Interfaces
{
    /// <summary>
    /// Interface représentant un point sur la carte avec des coordonnées géographiques
    /// </summary>
    public interface IMapPoint
    {
        int Id { get; }
        double Latitude { get; }
        double Longitude { get; }
    }
}
