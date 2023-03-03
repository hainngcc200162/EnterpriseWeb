using System.ComponentModel.DataAnnotations;

namespace EnterpriseWeb.Models
{
    public class IdeaCategory
    {
        public int Id { get; set; }
        public int? IdeaID { get; set; }
        public Idea? Idea { get; set; }
        public int? CategoryID { get; set; }
        public Category? Category { get; set; }
    }
}