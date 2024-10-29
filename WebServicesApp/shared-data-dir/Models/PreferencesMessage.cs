namespace Nsu.HackathonProblem.SharedData.Models;

public class PreferencesMessage
{
    public Employee Employee { get; set; }
    public Wishlist Preferences { get; set; }
    public string EmployeeType { get; set; } 
    public int HackathonId { get; set; }
}
