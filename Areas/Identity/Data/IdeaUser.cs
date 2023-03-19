using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnterpriseWeb.Models;
using Microsoft.AspNetCore.Identity;

namespace EnterpriseWeb.Areas.Identity.Data;

// Add profile data for application users by adding properties to the IdeaUser class
public class IdeaUser : IdentityUser
{
    [PersonalData]
    public string  Name { get; set; }

    [PersonalData]
    public DateTime  DOB { get; set; }

    [PersonalData]
    public string  Address { get; set; }
    [PersonalData]
    public byte[] ProfilePicture { get; set; }

    [PersonalData]
    public int? DepartmentID { get; set; }
    public Department? Department { get; set; }

    public ICollection<Comment>? Comments { get; set; }
    public ICollection<Idea>? Ideas { get; set; }
    public ICollection<Rating>? Ratings { get; set; }
    public ICollection<Viewing>? Viewings { get; set; }


}

