using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.Seguridad
{
    public class ValidNumEmpleadosModel
    {
        public int numEmpActivos { get; set; }
        public int numEmpLicenciados { get; set; }
        public long nextBadgenumber { get; set; }

        public ValidNumEmpleadosModel(int activos, int licenciados, long badgenumber)
        {
            numEmpActivos = activos;
            numEmpLicenciados = licenciados;
            this.nextBadgenumber = badgenumber;
        }
    }
}
