using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataAccessLibrary.Models.Empresa
{
    public class MailSettingsModel : IMailSettingsModel
    {
        // [Required(ErrorMessage = "Por favor ingrese una cuenta de correo")]
        public string FromEmail { get; set; }

        // [Required(ErrorMessage = "Por favor ingrese la contraseña")]
        public string Password { get; set; }

        // [Required(ErrorMessage = "Por favor ingrese el Host")]
        public string Host { get; set; }

        // [Required(ErrorMessage = "Por favor ingrese el puerto")]
        public string Port { get; set; }

        // [Required(ErrorMessage = "Por favor ingrese el valor de SSL")]
        public bool Ssl { get; set; }

        public int TimeOut { get; set; }


        public MailSettingsModel()
        {

        }

    }
}
