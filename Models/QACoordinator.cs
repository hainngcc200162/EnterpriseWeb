namespace EnterpriseWeb.Models;
public class QACoordiantor
{
    public int Id{ get ; set ; }

    public ICollection<QACoordinator>? QACoordinators { get; set; }

}