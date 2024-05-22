using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.SolicitudesPermiso
{
    public class AnulacionPermisoModel
    {

        public int IdSolicitud { get; set; }
        public string MotivoAnulacion { get; set; }
        public DateTime FAnulacion { get; set; }
    }
}
