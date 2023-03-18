using System.ComponentModel.DataAnnotations;
using EnterpriseWeb.Areas.Identity.Data;
namespace EnterpriseWeb.Models
{
    public class Rating
    {
        public int Id { get; set; }
        public int IdeaID { get; set; }
        public Idea? Idea { get; set; }
        public string? UserId { get; set; }
        public IdeaUser? IdeaUser { get; set; }
        public int? RatingUp { get; set; }
        public int? RatingDown { get; set; }

        [DataType(DataType.Date)]
        public DateTime SubmitionDate { get; set; }
    }
}