using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.Vacaciones
{
    public class CorteVacationModel
    {
        public int UserId { get; set; }
        public DateTime FechaCorte { get; set; }
        public string Motivo { get; set; }
        public DateTime date {get; set;}
        public double DiasAgregados { get; set; }
        public double DiasHabiles { get; set; }
        public double Disponible { get; set; }
        
        
        public CorteVacationModel()
        {
            FechaCorte = DateTime.Now;
        }

    }
}
