namespace Nsu.HackathonProblem.SharedData.Models;

public class HackathonAnnouncementMessage
{
    public int HackathonId { get; set; }
    public string Message { get; set; }
    public Guid QueryId { get; set; }
}