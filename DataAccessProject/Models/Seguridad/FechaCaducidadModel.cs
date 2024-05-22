using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.Seguridad
{
    public class FechaCaducidadModel
    {
        public DateTime Fecha { get; set; }

        public FechaCaducidadModel()
        {
            this.Fecha = new DateTime(2000,1,1);
        }
        public FechaCaducidadModel(DateTime date)
        {
            this.Fecha = date;
        }
    }
}
