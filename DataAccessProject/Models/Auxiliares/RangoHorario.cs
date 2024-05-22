using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.Auxiliares
{
    public  class RangoHorario
    {
        public DateTime Ini { get; set; }
        public DateTime Fin { get; set; }
        public RangoHorario(DateTime ini, DateTime fin)
        {
            Ini = ini;
            Fin = fin;
        }   

    }
}
