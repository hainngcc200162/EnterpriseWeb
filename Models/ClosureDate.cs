namespace EnterpriseWeb.Models;

public class ClosureDate{
    public int Id { get; set; }
    public int AcademicYear { get; set; }
    public DateTime ClousureDate { get; set; }
    public DateTime FinalDate { get; set; }

    public ICollection<Idea>? Ideas { get; set; }
}