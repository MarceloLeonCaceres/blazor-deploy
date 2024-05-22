using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.Reports
{
    public class PermisoModel
    {
        public int? UserId { get; set; }
        public DateTime? FIni { get; set; }
        public DateTime? FFin { get; set; }
        public int IdTipo { get; set; }
        public string? Motivo { get; set; }
        public DateTime FIngreso { get; set; }
        public int? IdSolicitud { get; set; }

        public string? CsvUsersId { get; set; }

        public PermisoModel(int? userId, DateTime? fIni, DateTime? fFin, int idTipo, string motivo, int idSolicitud, DateTime fIngreso)
        {
            UserId = userId;
            FIni = fIni;
            FFin = fFin;
            IdTipo = idTipo;
            Motivo = motivo;            
            IdSolicitud = idSolicitud;
            FIngreso = fIngreso;
            CsvUsersId = null;
        }   

        public PermisoModel()
        {
            FIngreso = DateTime.Now;
            Motivo = "";
        }

        public PermisoModel(string csvUsersId, DateTime? fIni, DateTime? fFin, int idTipo, string motivo, int idSolicitud, DateTime fIngreso)
        {
            UserId = null;            
            FIni = fIni;
            FFin = fFin;
            IdTipo = idTipo;
            Motivo = motivo;
            IdSolicitud = idSolicitud;
            FIngreso = fIngreso;
            CsvUsersId = csvUsersId;
        }

    }
}
