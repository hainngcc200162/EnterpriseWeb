using System.ComponentModel.DataAnnotations;

namespace EnterpriseWeb.Models
{
    public class Rating
    {
        public int Id { get; set; }
        public Idea IdeaId { get; set; }
        public User UserId { get; set; }
        public int RatingValue { get; set; }
        [DataType(DataType.Date)]
        public DateTime SubmittionDate { get; set; }
    }
}