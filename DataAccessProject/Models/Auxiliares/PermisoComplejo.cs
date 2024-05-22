using System;
using System.Collections.Generic;
using System.Text;
using DataAccessLibrary.Models.Reports;


namespace DataAccessLibrary.Models.Auxiliares
{
    public class PermisoComplejo
    {
        public int UserId { get; set; }
        public int IdTipo { get; set; }
        public string Motivo { get; set; }

        public int IdSolicitud { get; set; }    
        public List<RangoHorario> LstRangoHorarios { get; set; }

        public PermisoComplejo(PermisoModel permiso)
        {
            this.UserId = permiso.UserId.Value;
            this.IdTipo = permiso.IdTipo;
            this.Motivo = permiso.Motivo;
            this.IdSolicitud = permiso.IdSolicitud.HasValue ? permiso.IdSolicitud.Value : 0;
            this.LstRangoHorarios = new List<RangoHorario>();
            
            DateTime fechaD_aux;
            DateTime fechaH_aux;
            TimeSpan horaE;
            TimeSpan horaS;
            int difffechas = (permiso.FFin.Value.Date - permiso.FIni.Value.Date).Days;

            if (difffechas == 0)
            {
                LstRangoHorarios.Add(new RangoHorario(permiso.FIni.Value, permiso.FFin.Value));
            }
            else
            {
                for (int i = 0; i <= difffechas; i++)
                {
                    if (i == 0)
                    {
                        horaE = new TimeSpan(permiso.FIni.Value.Hour, permiso.FIni.Value.Minute, 0);
                        horaS = new TimeSpan(23, 59, 59);
                    }
                    else if (i == difffechas)
                    {
                        horaE = new TimeSpan(0, 0, 0);
                        horaS = new TimeSpan(permiso.FFin.Value.Hour, permiso.FFin.Value.Minute, 0);
                    }
                    else
                    {
                        horaE = new TimeSpan(0, 0, 0);
                        horaS = new TimeSpan(23, 59, 59);
                    }
                    fechaD_aux = permiso.FIni.Value.Date.AddDays(i).AddHours(horaE.Hours).AddMinutes(horaE.Minutes).AddSeconds(horaE.Seconds);
                    fechaH_aux = permiso.FIni.Value.Date.AddDays(i).AddHours(horaS.Hours).AddMinutes(horaS.Minutes).AddSeconds(horaS.Seconds);
                    LstRangoHorarios.Add(new RangoHorario(fechaD_aux, fechaH_aux));

                }
            }
        }

    }
}
