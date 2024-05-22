using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.Reports
{
    public class MarcacionManualModel
    {
        public int? UserId { get; set; }
        public DateTime Checktime { get; set; }
        
        public string CheckType { get; set; }
        public int IsAdd { get; set; }        
        public int IsModify { get; set; }
        public int IsDelete { get; set; }
        public int InCount { get; set; }
        public int IsCount { get; set; }
        public string Yuyin { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime Fecha { get; set; }

        public string? CsvUsersId { get; set; }

        public MarcacionManualModel(int userId, DateTime fechaHora, string tipo, string yuyin, string admin)
        {
            
            this.UserId = userId;
            this.Checktime = fechaHora;
            this.Fecha = DateTime.Now;
            this.CheckType = tipo;
            this.Yuyin = yuyin;
            this.ModifiedBy = admin;
        }

        public MarcacionManualModel(string csvUsersId, DateTime fechaHora, string tipo, string yuyin, string admin)
        {

            this.UserId = null;
            this.Checktime = fechaHora;
            this.Fecha = DateTime.Now;
            this.CheckType = tipo;
            this.Yuyin = yuyin;
            this.ModifiedBy = admin;
            this.CsvUsersId = csvUsersId;
        }


    }
    
}
