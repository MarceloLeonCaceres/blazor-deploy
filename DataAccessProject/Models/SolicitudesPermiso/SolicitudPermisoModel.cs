using DataAccessLibrary.Models.Reports;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.SolicitudesPermiso
{
    public class SolicitudPermisoModel : PermisoModel
    {

        // Estado Solicitud Permiso
        // -1 = Pendiente (recién creado)
        //  0 = Negada
        //  1 = Aprobada
        // -2 = Anulado

        public int Estado { get; set; }                
        public int IdSupervisor1 { get; set; }  
        public int? Respuesta1 { get; set; }
        public DateTime? FechaR1 { get; set; }
        public string? MotivoR1 { get; set; }
        public int IdSupervisor2 { get; set; }
        public int? Respuesta2 { get; set; }
        public DateTime? FechaR2 { get; set; }
        public string? MotivoR2 { get; set; }
        public string? Adjunto { get; set; }        

        public SolicitudPermisoModel(int? userId, DateTime? fIni, DateTime? fFin, int idTipo, string motivo, int idSolicitud) :
            base(userId, fIni, fFin, idTipo, motivo, idSolicitud, DateTime.Now)
        {
            //base.UserId = userId;
            //base.FIni = fIni;
            //base.FFin = fFin;
            //base.IdTipo = idTipo;
            //base.Motivo = motivo;
            //base.IdSolicitud = idSolicitud;
        }

        public SolicitudPermisoModel(int userId, int idSolicitud, DateTime fIngreso, int idTipo, DateTime fIni, DateTime fFin, string motivo, 
            DateTime? fechaR1, DateTime? fechaR2, string adjunto) :
            base(userId, fIni, fFin, idTipo, motivo, idSolicitud, fIngreso)
        {
            base.UserId = userId;
            base.FIni = fIni;
            base.FFin = fFin;
            base.IdTipo = idTipo;
            base.Motivo = motivo;
            base.IdSolicitud = idSolicitud;

            FechaR1 = fechaR1;
            FechaR2 = fechaR2;
            Adjunto = adjunto;
        }

        public SolicitudPermisoModel()
        {
        }


    }
}
