using System.ComponentModel.DataAnnotations;
using EnterpriseWeb.Areas.Identity.Data;

namespace EnterpriseWeb.Models;

public class Comment
{
    public int Id { get; set; }
    public string? CommentText { get; set; }
    [DataType(DataType.Date)]
    public DateTime SubmitDate { get; set; }
    public string? UserId { get; set; }
    public IdeaUser? IdeaUser { get; set; }
    public int IdeaId { get; set; }
    public Idea? Idea { get; set; }
    public int? status { get; set; }
}