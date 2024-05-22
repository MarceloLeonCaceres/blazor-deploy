using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.FeriadoLogTipoPermiso
{
    public class FeriadoModel
    {
        public int? HolidayId { get; set; }
        public string HolidayName { get; set; }
        public DateTime StartTime { get; set; }
        public int IdCiudad { get; set; }
        public string NomCiudad { get; set; }

        public FeriadoModel()
        {
            HolidayName = "Nuevo Feriado";
            StartTime = DateTime.Today;
            IdCiudad = 0;
            NomCiudad = "Nacional";
        }
    }
}
