public class User
{
    public int Id { get; set; }

    public string Prenom { get; set; }

    public string Nom { get; set; }
    
    public string Username { get; set; }

    public string Password { get; set; }

    public bool IsAdmin { get; set; }

    public Station? Station { get; set; }

    public EmployeeType Type { get; set; }
}
