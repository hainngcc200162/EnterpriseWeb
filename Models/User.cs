using System.ComponentModel.DataAnnotations;

namespace EnterpriseWeb.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
        public ICollection<Comment> CommentId { get; set; }
        public ICollection<Idea> IdeaId { get; set; }
        public ICollection<Rating> RatingId { get; set; }
    }
}