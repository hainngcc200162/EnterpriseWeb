using System.ComponentModel.DataAnnotations;
using EnterpriseWeb.Areas.Identity.Data;

namespace EnterpriseWeb.Models
{
    public class Idea
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? Status { get; set; }
        public int? IdeaStatus { get; set; }
        public string? UserId { get; set; }
        public IdeaUser? IdeaUser { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime SubmissionDate { get; set; }
        [DataType(DataType.Upload)]
        public string? SupportingDocuments { get; set; }
        public byte[] DataFile { get; set; }
        public int? DepartmentID { get; set; }
        public Department? Department { get; set; }
        public int? ClosureDateID { get; set; }
        public ClosureDate? ClosureDate { get; set; }
        public ICollection<IdeaCategory>? IdeaCategories { get; set; }
        public ICollection<Comment>? Comments { get; set; }
        public ICollection<Rating>? Ratings { get; set; }
        public ICollection<Viewing>? Viewings { get; set; }
    }
}