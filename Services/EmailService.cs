using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace SWP_BE.Services
{
    public interface IEmailService
    {
        Task SendTaskAssignmentEmailAsync(string toEmail, string toName, string taskName, string projectName, string deadline);
        Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetLink);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendTaskAssignmentEmailAsync(string toEmail, string toName, string taskName, string projectName, string deadline)
        {
            var emailSettings = _config.GetSection("EmailSettings");
            var smtpServer = emailSettings["SmtpServer"];
            var port = int.Parse(emailSettings["Port"]);
            var senderEmail = emailSettings["SenderEmail"];
            var senderName = emailSettings["SenderName"];
            var appPassword = emailSettings["AppPassword"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = $"[LabelMaster] Bạn có Task mới: {taskName}";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; border: 1px solid #ddd; padding: 20px; border-radius: 8px;'>
                    <h2 style='color: #2563eb;'>Thông báo nhiệm vụ mới</h2>
                    <p>Chào <b>{toName}</b>,</p>
                    <p>Bạn vừa được giao một nhiệm vụ mới trong dự án <b>{projectName}</b>.</p>
                    <ul style='background: #f8fafc; padding: 15px 30px; border-radius: 5px;'>
                        <li><b>Tên Task:</b> {taskName}</li>
                        <li><b>Hạn chót:</b> <span style='color: red; font-weight: bold;'>{deadline}</span></li>
                    </ul>
                    <p>Vui lòng đăng nhập hệ thống để bắt đầu làm việc.</p>
                </div>";

            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync(smtpServer, port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(senderEmail, appPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetLink)
        {
            var emailSettings = _config.GetSection("EmailSettings");
            var smtpServer = emailSettings["SmtpServer"];
            var port = int.Parse(emailSettings["Port"]);
            var senderEmail = emailSettings["SenderEmail"];
            var senderName = emailSettings["SenderName"];
            var appPassword = emailSettings["AppPassword"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = "[LabelMaster] Reset mật khẩu";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = $@"
        <div style='font-family: Arial; max-width:600px; border:1px solid #ddd; padding:20px'>
            <h2 style='color:#2563eb'>Reset Password</h2>
            <p>Chào <b>{toName}</b>,</p>
            <p>Bạn vừa yêu cầu đặt lại mật khẩu.</p>

            <p>
                <a href='{resetLink}'
                   style='background:#2563eb;color:white;padding:10px 20px;
                          text-decoration:none;border-radius:5px'>
                   Reset Password
                </a>
            </p>

            <p>Nếu bạn không yêu cầu thao tác này, hãy bỏ qua email.</p>
        </div>";

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpServer, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(senderEmail, appPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}