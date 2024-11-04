namespace Nsu.HackathonProblem.SharedData.Models;

public class EmployeePreferenceEntity
{
    public int HackathonId { get; set; }
    public Role Role { get; set; }
    public int EmployeeId { get; set; }
    public int PreferredEmployeeId { get; set; }
    public int Priority { get; set; }
    
    public static (Role role, int EmployeeId, int HackathonId, int
        PreferredEmployeeId) GetKey(PreferencesMessage e, int i) 
        => ( e.EmployeeType == "Junior"? Role.Junior:Role.TeamLead, e.Employee.Id, e.HackathonId, e.Preferences.DesiredEmployees[i]);
    

}