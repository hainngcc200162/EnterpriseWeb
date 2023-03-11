using System.ComponentModel.DataAnnotations;
using EnterpriseWeb.Areas.Identity.Data;

namespace EnterpriseWeb.Models;

public class Viewing
{
    public int Id { get; set; }
    public int? Count { get; set; }
    [DataType(DataType.Date)]
    public DateTime ViewDate { get; set; }
    public string? UserId { get; set; }
    public IdeaUser? IdeaUser { get; set; }
    public int IdeaId { get; set; }
    public Idea? Idea { get; set; }
}