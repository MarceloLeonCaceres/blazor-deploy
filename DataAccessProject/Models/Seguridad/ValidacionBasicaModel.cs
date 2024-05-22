using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.Seguridad
{
    public class SeguridadEmpleAdminFecha : FechaCaducidadModel
    {
        public int iNumAdmin { get; set; }
        public int iNumEmpleados { get; set; }

        public SeguridadEmpleAdminFecha()
        {
        }
        public SeguridadEmpleAdminFecha(int iNumEmpleados, int iNumAdmin, DateTime Fecha)
        {
            this.Fecha = Fecha;
            this.iNumAdmin = iNumAdmin;
            this.iNumEmpleados = iNumEmpleados;
        }
    }
}
