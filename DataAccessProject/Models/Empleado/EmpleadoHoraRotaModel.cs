using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.Empleado
{
    public class EmpleadoHoraRotaModel : EmpleadoBaseModel
    {
        public int IdHorario { get; set; }
        public string NomHorario { get; set; }
        public DateTime FechaIni { get; set; }
        public DateTime FechaFin { get; set; }

        public EmpleadoHoraRotaModel()
        {
            NomHorario = "";
            FechaIni = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            FechaFin = DateTime.Now;
        }

        
    }
}
