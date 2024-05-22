using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.Reports
{
    public class RpPermisosModel
    {
        public int UserId { get; set; }
        public string NumCA { get; set; }
        public string Cedula { get; set; }
        public string Empleado { get; set; }
        public string Departamento { get; set; }
        public string Dia { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }
        public DateTime Tiempo { get; set; }

        public string Motivo { get; set; }
        public string TipoPermiso { get; set; }
        public string CategoriaPermiso { get; set; }

        public int IdSolicitud { get; set; }

    }
}
