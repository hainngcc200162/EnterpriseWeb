using System.ComponentModel.DataAnnotations;

namespace EnterpriseWeb.Models
{
    public class Idea
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? UserID { get; set; }
        public User? User { get; set; }

        [DataType(DataType.Date)]
        public DateTime SubmissionDate { get; set; }
        public string? SupportingDocuments { get; set; }
        public int? DepartmentID { get; set; }
        public Department? Department { get; set; }
        public int? ClosureDateID { get; set; }
        public ClosureDate? ClosureDate { get; set; }
        public ICollection<IdeaCategory>? IdeaCategories { get; set; }
        public ICollection<Comment>? Comments { get; set; }
        public ICollection<Rating>? Ratings { get; set; }
    }
}