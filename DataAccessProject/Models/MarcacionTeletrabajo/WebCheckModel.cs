using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLibrary.Models.MarcacionTeletrabajo
{
    public class WebCheckModel : IWebCheckModel
    {
        public int LogId { get; set; }
        public int UserId { get; set; }
        public DateTime Checktime { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public byte[] Fotografia { get; set; }
        public string Comentario { get; set; }
        public int Estado { get; set; }

        public WebCheckModel()
        {
        }

        public WebCheckModel(int userId, DateTime checktime, float latitude, float longitude, byte[] fotografia, string comentario)
        {
            UserId = userId;
            Checktime = checktime;
            Latitude = latitude;
            Longitude = longitude;
            Fotografia = fotografia;
            Comentario = comentario;

            Estado = -1;
        }
    }
}
