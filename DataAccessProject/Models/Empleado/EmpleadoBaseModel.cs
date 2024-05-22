using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.Models
{
    public class EmpleadoBaseModel : IEmpleadoBaseModel
    {
        public int? UserId { get; set; }
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Solo números")]
        public string Badge { get; set; }

        [RegularExpression(@"^[a-zA-Z0-9áéíóúñÁÉÍÓÚÑ\s.]+$", ErrorMessage = "Solo caracteres alfanuméricos")]
        public string NombreEmp { get; set; }
        public int DeptId { get; set; }
        public string Departamento { get; set; }

        public EmpleadoBaseModel()
        {
        }

    }
}
