using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace EnterpriseWeb.Models
{
    public class CategorySearchView{
        public List<IdeaCategory>? IdeaCategories { get; set; }
        public List<Idea>? Ideas { get; set; }
        public List<Category> Categories { get; set; }
        public string? IdeaCategory { get; set; }
        public string? Search { get; set; } 


        // public List<Book>? Books { get; set; }
        // public SelectList? Categories { get; set; }
        // public string? BookCategory { get; set; }
        // public string? Search { get; set; }

    }
}