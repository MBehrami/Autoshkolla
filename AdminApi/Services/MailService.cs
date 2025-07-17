using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AdminApi.Models;
using AdminApi.Models.Helper;
using AdminApi.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AdminApi.Services
{
    public class MailService : IMailService
    {
        private readonly AppDbContext _context;
        public MailService(AppDbContext context)
        {
            _context=context;
        }
        public async Task SendPasswordEmailAsync(ForgetPassword request)
        {
            var emailSettings=_context.SiteSettings.OrderBy(q=>q.SiteSettingsId)
            .Select(p=>new MailSettings{SiteTitle=p.SiteTitle,Mail=p.DefaultEmail,DisplayName=p.DisplayName,Password=p.Password,Host=p.Host,
            Port=p.Port,ForgetPasswordEmailSubject=p.ForgetPasswordEmailSubject,ForgetPasswordEmailHeader=p.ForgetPasswordEmailHeader,
            ForgetPasswordEmailBody=p.ForgetPasswordEmailBody}).First();

            emailSettings.ForgetPasswordEmailBody=emailSettings.ForgetPasswordEmailBody==null?"":emailSettings.ForgetPasswordEmailBody.Replace("[siteTitle]",request.SiteTitle).Replace("[password]",request.Password);
            string filePath = Path.Combine(Directory.GetCurrentDirectory(),"Resources","EmailTemplate","forgetPassword.html");
            StreamReader str = new StreamReader(filePath);
            string MailText = str.ReadToEnd();
            str.Close();
            MailText = MailText.Replace("[subject]",emailSettings.ForgetPasswordEmailSubject).Replace("[logoPath]",request.LogoPath).Replace("[siteUrl]", request.SiteUrl).Replace("[header]", emailSettings.ForgetPasswordEmailHeader).Replace("[body]", emailSettings.ForgetPasswordEmailBody).Replace("[resetLink]",request.ResetLink);
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(emailSettings.SiteTitle,emailSettings.Mail));
            email.To.Add(MailboxAddress.Parse(request.ToEmail));
            email.Subject=emailSettings.ForgetPasswordEmailSubject;
            var builder = new BodyBuilder();
            builder.HtmlBody = MailText;
            email.Body = builder.ToMessageBody();
            using var smtp = new SmtpClient();
            smtp.LocalDomain="localhost";
            smtp.Connect(emailSettings.Host, emailSettings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(emailSettings.Mail, emailSettings.Password);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }
        public async Task SendWelcomeEmailAsync(WelcomeRequest request)
        {
            var emailSettings=_context.SiteSettings.OrderBy(q=>q.SiteSettingsId)
            .Select(p=>new MailSettings{SiteTitle=p.SiteTitle,Mail=p.DefaultEmail,DisplayName=p.DisplayName,Password=p.Password,Host=p.Host,
            Port=p.Port,WelcomeEmailSubject=p.WelcomeEmailSubject,WelcomeEmailHeader=p.WelcomeEmailHeader,
            WelcomeEmailBody=p.WelcomeEmailBody}).First();

            emailSettings.WelcomeEmailBody=emailSettings.WelcomeEmailBody==null?"":emailSettings.WelcomeEmailBody.Replace("[siteTitle]",request.SiteTitle).Replace("[password]",request.Password).Replace("[email]",request.ToEmail);
            string filePath = Path.Combine(Directory.GetCurrentDirectory(),"Resources","EmailTemplate","welcome.html");
            StreamReader str = new StreamReader(filePath);
            string MailText = str.ReadToEnd();
            str.Close();
            MailText = MailText.Replace("[subject]",emailSettings.WelcomeEmailSubject).Replace("[logoPath]",request.LogoPath).Replace("[siteUrl]", request.SiteUrl).Replace("[header]", emailSettings.WelcomeEmailHeader).Replace("[body]", emailSettings.WelcomeEmailBody);
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(emailSettings.SiteTitle,emailSettings.Mail));
            email.To.Add(MailboxAddress.Parse(request.ToEmail));
            email.Subject = emailSettings.WelcomeEmailSubject;
            var builder = new BodyBuilder();
            builder.HtmlBody = MailText;
            email.Body = builder.ToMessageBody();
            using var smtp = new SmtpClient();
            smtp.LocalDomain="localhost";
            smtp.Connect(emailSettings.Host, emailSettings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(emailSettings.Mail, emailSettings.Password);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }
    }
}