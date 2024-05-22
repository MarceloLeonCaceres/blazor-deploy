using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.Reports
{
    public class RpMarcacionesModel
    {
        public int UserId { get; set; }
        public string NumCA { get; set; }
        public string Empleado { get; set; }
        public string Departamento { get; set; }
        public string Dia { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime Hora { get; set; }
        public string Estado { get; set; }
        public string Verificacion { get; set; }
        public string sn { get; set; }
        public string Reloj { get; set; }
        public string Cedula { get; set; }
        public string Cargo { get; set; }
        public string Ciudad { get; set; }
        public string CentroCostos { get; set; }


    }
}
