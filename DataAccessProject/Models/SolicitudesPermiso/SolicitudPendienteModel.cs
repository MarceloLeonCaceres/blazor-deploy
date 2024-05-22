using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.SolicitudesPermiso
{
    public class SolicitudPendienteModel : SolicitudPermisoModel
    {

        public string NumCA { get; set; }
        public string NomEmpleado { get; set; }
        public string TipoPermiso { get; set; }

        public string NomSupervisor1 { get; set; }
        public string NomSupervisor2 { get; set; }

        //public SolicitudPendienteModel(int? userId, DateTime fIni, DateTime fFin, int idTipo, string motivo, int idSolicitud, DateTime fIngreso ) :
        //    base(userId, fIni, fFin, idTipo, motivo, idSolicitud, fIngreso)
        //{
        //}
        
        public SolicitudPendienteModel(int userId, string numCA, string nomEmpleado, int idSolicitud, int idTipo,
            DateTime fIni, DateTime fFin, string motivo, DateTime fIngreso, string tipoPermiso, 
            string nomSupervisor1, DateTime? fechaR1, string nomSupervisor2, DateTime? fechaR2, string adjunto) :
            base(userId, idSolicitud, fIngreso, idTipo, fIni, fFin, motivo, fechaR1, fechaR2, adjunto)
        {
            this.NumCA = numCA;
            this.NomEmpleado = nomEmpleado;
            this.TipoPermiso = tipoPermiso;
            this.NomSupervisor1 = nomSupervisor1;
            this.NomSupervisor2 = nomSupervisor2;
        }
       
    }


}
