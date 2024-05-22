using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.HorarioTurnoRegla
{
    public class TurnoModel
    {
        public int? Id { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int Ciclo { get; set; }
        public int Unidades { get; set; }
        public int Horas { get; set; }
        public string SUnidades { get; set; }
        public TurnoModel()
        {
            Id = -1;
            Nombre = "Nuevo Turno";
            FechaInicio = new DateTime(2021, 1, 1);
            FechaFin = new DateTime(2030, 1, 1);
            Ciclo = 1;
            Unidades = 1;
            Horas = 40;
            SUnidades = "Semana";
        }
    }
}
