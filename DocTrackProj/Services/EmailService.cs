using System.Net;
using System.Net.Mail;

namespace DocTrackProj.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendOtpAsync(string toEmail, string code)
        {
            var host = _config["Smtp:Host"] ?? "smtp.gmail.com";
            var port = int.Parse(_config["Smtp:Port"] ?? "587");
            var senderEmail = _config["Smtp:SenderEmail"] ?? throw new Exception("Smtp:SenderEmail missing");
            var senderName = _config["Smtp:SenderName"] ?? "CTU DocTrack";
            var appPass = _config["Smtp:AppPassword"] ?? throw new Exception("Smtp:AppPassword missing");

            var mail = new MailMessage();
            mail.From = new MailAddress(senderEmail, senderName);
            mail.To.Add(toEmail);
            mail.Subject = "CTU DocTrack - Verification Code";
            mail.Body = $@"<h2>CTU Document Tracking</h2>
            <p>Your verification code is:</p>
            <h1 style='letter-spacing:5px; color:#0d6efd;'>{code}</h1>
            <p>This code expires in 5 minutes. Do not share.</p>";
            mail.IsBodyHtml = true;

            using var smtp = new SmtpClient(host, port);
            smtp.Credentials = new NetworkCredential(senderEmail, appPass);
            smtp.EnableSsl = true;
            await smtp.SendMailAsync(mail);
        }
    }
}