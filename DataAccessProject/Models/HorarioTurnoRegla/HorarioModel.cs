using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.HorarioTurnoRegla
{
    public class HorarioModel : IComparable<HorarioModel>
    {
        public int? Id { get; set; }
        public string Nombre { get; set; }
        public DateTime Entrada { get; set; }
        public DateTime Salida { get; set; }
        public DateTime IniEntrada { get; set; }
        public DateTime FinEntrada { get; set; }
        public DateTime IniSalida { get; set; }
        public DateTime FinSalida { get; set; }
        public int GraciaEntrada { get; set; }
        public int GraciaSalida { get; set; }
        public int CheckIn { get; set; }
        public int CheckOut { get; set; }
        public int Color { get; set; }
        public double DiaTrabajo { get; set; }
        public int MinutosBreak { get; set; }
        public int DescuentaAlmuerzo { get; set; }
        public int Tipo { get; set; }
        public int OverTime { get; set; }
        public int Duracion { get; set; }
        public string SDescuentaAlmuerzo { get; set; }
        public string STipo { get; set; }
        public string SDuracion { get; set; }

        public HorarioModel()
        {
            Nombre = "Nuevo Horario";

            Entrada = new DateTime(1899, 12, 30, 8, 0, 0);
            Salida = new DateTime(1899, 12, 30, 17, 0, 0);
            IniEntrada = new DateTime(1899, 12, 30, 6, 0, 0);
            FinEntrada = new DateTime(1899, 12, 30, 12, 30, 0);
            IniSalida = new DateTime(1899, 12, 30, 14, 31, 0);
            FinSalida = new DateTime(1899, 12, 30, 21, 0, 0);
            DiaTrabajo = 1;
            GraciaEntrada = 0;
            GraciaSalida = 0;
            MinutosBreak = 60;
            DescuentaAlmuerzo = 1;
            Tipo = 0;
            OverTime = 0;
            Duracion = -1;
            CheckIn = 0;
            CheckOut = 0;

        }

        public int CompareTo(HorarioModel otro)
        {
            if (otro == null)
                return 1;
            else
            {
                return Entrada.CompareTo(otro.Entrada);
            }
        }

        public bool Equals(HorarioModel otroHorario)
        {
            if (otroHorario == null) return false;
            return Entrada.Equals(otroHorario.Entrada);

        }

        public static int CompareHorarios(HorarioModel x, HorarioModel y)
        {
            // iguales => 0
            // x > y   => 1
            // x < y   => -1
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (y == null)
                {
                    return 1;
                }
                else
                {
                    return x.Entrada.CompareTo(y.Entrada);
                }
            }

        }
    }
}
