using System.ComponentModel.DataAnnotations;

namespace EnterpriseWeb.Models
{
    public class Rating
    {
        public int Id { get; set; }
        public int IdeaID { get; set; }
        public Idea? Idea { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public int RatingValue { get; set; }

        [DataType(DataType.Date)]
        public DateTime SubmitionDate { get; set; }
    }
}