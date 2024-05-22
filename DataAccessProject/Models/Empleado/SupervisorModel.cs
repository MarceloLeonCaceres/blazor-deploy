using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.Empleado
{
    public class SupervisorModel : EmpleadoBaseModel
    {                

        public bool esAdministrador { get; set; }
        public bool esAprobador { get; set; }
        public bool esSupervisor3 { get; set; }
        public bool esSupervisorXl { get; set; }
    }
}
