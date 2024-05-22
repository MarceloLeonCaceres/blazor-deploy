using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.HorarioTurnoRegla
{
    public class DetalleTurnoModel
    {
        public int IdTurno { get; set; }
        public DateTime hDesde { get; set; }
        public DateTime hHasta { get; set; }
        public int sDia { get; set; }
        public int eDay { get; set; }
        public int IdHorario { get; set; }

        public DetalleTurnoModel()
        {
            IdTurno = -1;
            hDesde = new DateTime(1899, 12, 30);
            hHasta = new DateTime(1899, 12, 30);
            sDia = -1;
            eDay = -1;
            IdHorario = -1;
        }

        public DetalleTurnoModel(int idTurno, DateTime hDesde, DateTime hHasta, int sDia, int eDay, int idHorario)
        {
            IdTurno = IdTurno;
            this.hDesde = new DateTime(1899, 12, 30, hDesde.Hour, hDesde.Minute, 0);
            this.hHasta = new DateTime(1899, 12, 30, hHasta.Hour, hHasta.Minute, 0);
            this.sDia = sDia;
            this.eDay = hDesde > hHasta ? sDia + 1 : sDia;
            IdHorario = idHorario;
        }

        public DetalleTurnoModel(int idTurno, int sDia, int eDay, int idHorario)
        {
            IdTurno = idTurno;
            hDesde = new DateTime(1899, 12, 30);
            hHasta = new DateTime(1899, 12, 30);
            this.sDia = sDia;
            this.eDay = hDesde > hHasta ? sDia + 1 : sDia;
            IdHorario = idHorario;
        }
    }
}
