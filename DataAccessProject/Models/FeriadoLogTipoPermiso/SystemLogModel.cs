using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.FeriadoLogTipoPermiso
{
    public class SystemLogModel
    {
        public int ID { get; set; }
        public string Operator { get; set; }
        public DateTime LogTime { get; set; }

        public string Alias { get; set; }
        public int LogTag { get; set; }
        public string LogDescr { get; set; }
        public string LogDetailed { get; set; }
    }
}
