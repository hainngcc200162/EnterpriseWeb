using System.ComponentModel.DataAnnotations;

namespace EnterpriseWeb.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public ICollection<IdeaCategory>? IdeaCategories { get; set; }
    }
}