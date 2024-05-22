using System;

namespace DataAccessLibrary.Models.MarcacionTeletrabajo
{
    public interface IWebCheckModel
    {
        
        int LogId { get; set; }
        int UserId { get; set; }
        float Longitude { get; set; }
        float Latitude { get; set; }
        DateTime Checktime { get; set; }
        string Comentario { get; set; }
        byte[] Fotografia { get; set; }
        public int Estado { get; set; }
    }
}