using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.SolicitudesPermiso
{
    public  class DetalleSolicitudModel : SolicitudPermisoModel
    {
        public string NumCA { get; set; }
        public string NomEmpleado { get; set; }
        public string LeaveName { get; set; }
        public string CorreoEmpleado { get; set; }        
        public string sEstado { get; set; }

        public string NombreS1 { get; set; }
        public string NombreS2 { get; set; }
        public string CorreoS1 { get; set; }
        public string CorreoS2 { get; set; }
        public string sRespuesta1 { get; set; }
        public string sRespuesta2 { get; set; }
    }
}
