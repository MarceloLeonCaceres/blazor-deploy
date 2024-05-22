using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.Vacaciones
{
    public class EmpleadoContratoVacacionModel
    {
        public int UserId { get; set; }
        public string NumCA { get; set; }
        public string NomEmpleado { get; set; }
        public int IdContrato { get; set; }
        public string NomContrato { get; set; }
        public DateTime FechaEmpleo { get; set; }

    }
}
