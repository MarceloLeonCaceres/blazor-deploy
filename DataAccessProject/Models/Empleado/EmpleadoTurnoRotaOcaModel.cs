using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.Empleado
{
    public class EmpleadoTurnoRotaOcaModel : EmpleadoBaseModel
    {
        public int? IdTurno { get; set; }
        public string NomTurno { get; set; }
        public DateTime? FechaIni { get; set; }
        public DateTime? FechaFin { get; set; }

        public int? IdRotativo { get; set; }
        public string NomRotativo { get; set; }

        public int? IdOcasional { get; set; }
        public string NomOcasional { get; set; }


        public EmpleadoTurnoRotaOcaModel()
        {
            NomTurno = "";
            // FechaIni = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            // FechaFin = DateTime.Now;
        }

        public EmpleadoTurnoRotaOcaModel(string userid)
        {
            int UserId = int.Parse(userid);
            base.UserId = UserId;
            IdTurno = -1;
            FechaIni = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            FechaFin = DateTime.Now;
            IdRotativo = -1;
            IdOcasional = -1;

        }
    }

}
