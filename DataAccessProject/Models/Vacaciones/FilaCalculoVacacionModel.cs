using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.Vacaciones
{
    public class FilaCalculoVacacionModel : IComparable<FilaCalculoVacacionModel>
    {
        public int UserId { get; set; }
        public DateTime? InicioP { get; set; }
        public DateTime? FinP { get; set; }
        public int DateId { get; set; }
        public string Motivo { get; set; }
        public DateTime Fecha { get; set; }
        public double? VacGanadas { get; set; }
        public double? DiasAdicionales { get; set; }
        public double? VacTomadas { get; set; }
        public double? VacDescontadas { get; set; }
        public double? Disponible { get; set; }
        public string Dia { get; set; }
        public string NombrePermiso { get; set; } 

        public int TipoVacacion { get; set; }

        public FilaCalculoVacacionModel()
        {            
        }

        public FilaCalculoVacacionModel(string nombrePermiso, string motivo, DateTime fecha, double vacGanadas, double diasAdicionales)
        {
            NombrePermiso = nombrePermiso;
            Motivo = motivo;
            Fecha = fecha;
            VacGanadas = vacGanadas;
            DiasAdicionales = diasAdicionales;

        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public int CompareTo(FilaCalculoVacacionModel otraFila)
        {
            if(otraFila == null)
            {
                return 1;
            }
            else
            {
                if(otraFila.Fecha != null)
                {
                    return this.Fecha.CompareTo(otraFila.Fecha);
                }
                else
                {
                    return -1;
                }                
            }
        }

    }
}
