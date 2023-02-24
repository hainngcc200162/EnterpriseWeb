using System.ComponentModel.DataAnnotations;

namespace EnterpriseWeb.Models;

public class Comment{
    public int Id { get; set; }
    public string? CommentText { get; set; }
    [DataType(DataType.Date)]
    public DateTime SubmitDate { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int IdeaId { get; set; }
    public Idea? Idea { get; set; } 
}