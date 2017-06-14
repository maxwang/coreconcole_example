namespace DataImporter.Framework.Services
{
    public class SMTPOptions
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string LocalDomain { get; set; }
        public string SMTPSeverIP { get; set; }
        public string From { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
    }
}