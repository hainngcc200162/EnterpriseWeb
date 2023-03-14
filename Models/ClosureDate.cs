using System.ComponentModel.DataAnnotations;

namespace EnterpriseWeb.Models;

public class ClosureDate{
    public int Id { get; set; }
    public string? Name { get; set; }
    [DataType(DataType.Date)]
    public DateTime AcademicYear { get; set; }
    [DataType(DataType.Date)]
    public DateTime ClousureDate { get; set; }
    [DataType(DataType.Date)]
    public DateTime FinalDate { get; set; }

    public ICollection<Idea>? Ideas { get; set; }
}