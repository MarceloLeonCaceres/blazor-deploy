using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.Empleado
{
    public class AsignacionSupervisoresModel : EmpleadoBaseModel
    {
        public int TipoEmpleado { get; set; }
        public int? idSupervisor1 { get; set; }
        public int? idSupervisor2 { get; set; }
        public int? idSupervisor3 { get; set; }
        public string Supervisor1 { get; set; }
        public string Supervisor2 { get; set; }
        public string Supervisor3 { get; set; }



    }
}
