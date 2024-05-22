using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models
{
    public class EmpleadoAsignacionModel : EmpleadoBaseModel
    {
        public int idParametroAsignado { get; set; }
        public string nombreParametro { get; set; }
                
    }
}
