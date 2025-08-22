using Auth.Business.Models;
using Auth.Business.Utilies.Templates;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Auth.Business.Helpers.HelperServices.Email
{
    public class EmailService(IOptions<SmtpSettings> smtpSettings, EmailTemplate _emailTemplate)
    {
        private readonly SmtpSettings _smtpSettings = smtpSettings.Value;


        public async Task SendResetPassword(string toEmail, string fullName, string token)
        {
            await SendEmailAsync(toEmail, new EmailMessage
            {
                Subject = "Şifrənizi yeniləyin...",
                Content = _emailTemplate.ResetPassword(token, fullName)
            });
        }

        public async Task SendRegister(string toEmail, string fullName)
        {
            await SendEmailAsync(toEmail, new EmailMessage
            {
                Subject = "Xoş gəldiniz...",
                Content = _emailTemplate.RegisterCompleted(fullName)
            });
        }

        public async Task SendCreatedByAdmin(string toEmail, string fullName, string password)
        {
            await SendEmailAsync(toEmail, new EmailMessage
            {
                Subject = "Hesabınız yaradıldı...",
                Content = _emailTemplate.CreatedByAdmin(fullName, password)
            });
        }

        public async Task SendChangedPasswordByAdmin(string toEmail, string fullName, string password)
        {
            await SendEmailAsync(toEmail, new EmailMessage
            {
                Subject = "Şifrəniz yeniləndi...",
                Content = _emailTemplate.ChangedPasswordByAdmin(fullName, password)
            });
        }

        private async Task SendEmailAsync(string toEmail, EmailMessage emailMessage)
        {
            var fromAddress = new MailAddress(_smtpSettings.Username, "Template");
            var toAddress = new MailAddress(toEmail);
            string fromPassword = _smtpSettings.Password;
            string subject = emailMessage.Subject;
            string body = emailMessage.Content;

            var smtp = new SmtpClient
            {
                Host = _smtpSettings.Server,
                Port = _smtpSettings.Port,
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                IsBodyHtml = true,
                Body = body,
            })
                smtp.Send(message);
        }
    }
}