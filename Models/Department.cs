namespace EnterpriseWeb.Models;
public class Department
{
    public int Id { get ; set ; }
    
    public string? Name { get ; set ; }

    public string? Description { get ; set ; }
    public string? UserId { get; set; }
    
    public int? QACoordinatorID { get; set; }
    public QACoordinator? QACoordinator { get; set; }
}