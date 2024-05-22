using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.Empleado
{
    public class EmpleadoHoraOcasionalModel : EmpleadoHoraRotaModel
    {
        public int Type { get; set; }
        public int Flag { get; set; }
        public int Overtime { get; set; }

    }
}
