namespace EnterpriseWeb.Models;
public class QACoordiantor
{
    public int Id{ get ; set ; }

    public ICollection<Department>? Departments { get; set; }

}