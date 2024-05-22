using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.MarcacionTeletrabajo
{
    public class WebCheckDisplayModel : IWebCheckModel
    {
        public int LogId { get; set; }
        public int UserId { get; set; }
        public DateTime Checktime { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public byte[] Fotografia { get; set; }
        public string Comentario { get; set; }
        public int Estado { get; set; }

        public string NomEmpleado { get; set; }
        public string Badge { get; set; }

        public WebCheckDisplayModel()
        {

        }
    }
}
