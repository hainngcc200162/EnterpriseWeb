namespace EnterpriseWeb.Models;
public class QACoordinator
{
    public int Id{ get ; set ; }

    public ICollection<Department>? Departments { get; set; }

}