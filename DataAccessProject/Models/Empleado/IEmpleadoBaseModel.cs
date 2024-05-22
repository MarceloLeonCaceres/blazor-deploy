namespace DataAccessLibrary.Models
{
    public interface IEmpleadoBaseModel
    {
        string Badge { get; set; }
        string Departamento { get; set; }
        int DeptId { get; set; }
        string NombreEmp { get; set; }
        int? UserId { get; set; }
    }
}