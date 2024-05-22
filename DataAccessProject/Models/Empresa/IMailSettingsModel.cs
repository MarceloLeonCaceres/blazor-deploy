namespace DataAccessLibrary.Models.Empresa
{
    public interface IMailSettingsModel
    {
        string FromEmail { get; set; }
        string Host { get; set; }
        string Password { get; set; }
        string Port { get; set; }
        bool Ssl { get; set; }
        int TimeOut { get; set; }
    }
}