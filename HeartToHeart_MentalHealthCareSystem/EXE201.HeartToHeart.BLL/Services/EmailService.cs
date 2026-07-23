using EXE201.HeartToHeart.BLL.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly SmtpClient _smtpClient;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _smtpClient = ConfigureSmtpClient();
        }

        public async Task<bool> SendEmailConfirmationAsync(string email, string firstName, string confirmationToken, Guid userId)
        {
            try
            {
                var subject = "Confirm Your Email - HeartToHeart";
                var confirmationLink = $"{_configuration["AppSettings:BaseUrl"]}/api/Auth/confirm-email?userId={userId}&token={Uri.EscapeDataString(confirmationToken)}";

                var body = $@"
                                <!DOCTYPE html>
                                <html lang=""vi"">
                                <head>
                                    <meta charset=""UTF-8"">
                                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                    <title>Xác Nhận Email - HeartToHeart</title>
                                    <style>
                                        @import url('https://fonts.googleapis.com/css2?family=Poppins:wght@300;400;600;700&display=swap');
                                        .gradient-bg {{ background: linear-gradient(135deg, #FFFFFF 0%, #FADEDA 50%, #ACB9E7 100%); min-height: 100vh; padding: 40px 20px; }}
                                        .email-container {{ max-width: 600px; margin: 0 auto; background: rgba(255, 255, 255, 0.95); border-radius: 20px; box-shadow: 0 20px 40px rgba(172, 185, 231, 0.3); overflow: hidden; backdrop-filter: blur(10px); }}
                                        .header-section {{ background: linear-gradient(90deg, #ACB9E7, #FADEDA); padding: 40px 30px; text-align: center; position: relative; }}
                                        .header-section::before {{ content: ''; position: absolute; top: 0; left: 0; right: 0; bottom: 0; background: linear-gradient(45deg, rgba(250, 222, 218, 0.1), rgba(172, 185, 231, 0.1)); z-index: 1; }}
                                        .header-content {{ position: relative; z-index: 2; }}
                                        .logo {{ width: 80px; height: 80px; background: linear-gradient(135deg, #FFFFFF, #FADEDA); border-radius: 50%; margin: 0 auto 20px; display: flex; align-items: center; justify-content: center; box-shadow: 0 10px 30px rgba(172, 185, 231, 0.4); }}
                                        .heart-icon {{ font-size: 36px; color: #ACB9E7; }}
                                        .main-title {{ color: #4a4a4a; font-size: 28px; font-weight: 700; margin: 0; text-shadow: 0 2px 4px rgba(172, 185, 231, 0.3); }}
                                        .content-section {{ padding: 40px 30px; color: #4a4a4a; }}
                                        .welcome-text {{ font-size: 18px; font-weight: 600; color: #6b6b6b; margin-bottom: 20px; text-align: center; }}
                                        .description-text {{ font-size: 16px; line-height: 1.8; margin-bottom: 30px; text-align: center; color: #6b6b6b; }}
                                        .button-container {{ text-align: center; margin: 40px 0; }}
                                        .confirm-button {{ background: linear-gradient(135deg, #ACB9E7, #FADEDA); color: #4a4a4a; padding: 16px 40px; text-decoration: none; border-radius: 50px; display: inline-block; font-weight: 600; font-size: 16px; box-shadow: 0 10px 30px rgba(172, 185, 231, 0.4); transition: all 0.3s ease; border: 2px solid transparent; }}
                                        .confirm-button:hover {{ transform: translateY(-2px); box-shadow: 0 15px 40px rgba(172, 185, 231, 0.6); }}
                                        .link-section {{ background: linear-gradient(135deg, rgba(255, 255, 255, 0.8), rgba(250, 222, 218, 0.3)); padding: 20px; border-radius: 15px; margin: 20px 0; border: 1px solid rgba(172, 185, 231, 0.2); }}
                                        .link-text {{ font-size: 14px; color: #6b6b6b; margin-bottom: 10px; }}
                                        .link-url {{ word-break: break-all; color: #ACB9E7; font-size: 14px; font-weight: 500; }}
                                        .expiry-notice {{ background: linear-gradient(135deg, rgba(250, 222, 218, 0.3), rgba(172, 185, 231, 0.2)); padding: 15px; border-radius: 10px; border-left: 4px solid #FADEDA; margin: 20px 0; font-size: 14px; color: #6b6b6b; }}
                                        .footer-section {{ background: linear-gradient(135deg, rgba(172, 185, 231, 0.1), rgba(250, 222, 218, 0.1)); padding: 20px 30px; text-align: center; border-top: 1px solid rgba(172, 185, 231, 0.2); }}
                                        .footer-text {{ font-size: 12px; color: #999; margin: 0; }}
                                        .decorative-element {{ text-align: center; margin: 20px 0; }}
                                        .dots {{ display: inline-block; width: 8px; height: 8px; background: #FADEDA; border-radius: 50%; margin: 0 5px; }}
                                        .dots:nth-child(2) {{ background: #ACB9E7; }}
                                        .dots:nth-child(3) {{ background: #FADEDA; }}
                                    </style>
                                </head>
                                <body style='font-family: ""Poppins"", Arial, sans-serif; margin: 0; padding: 0;'>
                                    <div class=""gradient-bg"">
                                        <div class=""email-container"">
                                            <div class=""header-section"">
                                                <div class=""header-content"">
                                                    <div class=""logo""><span class=""heart-icon"">💖</span></div>
                                                    <h1 class=""main-title"">Chào Mừng Đến Với HeartToHeart!</h1>
                                                </div>
                                            </div>
                                            <div class=""content-section"">
                                                <p class=""welcome-text"">Xin chào {firstName},</p>
                                                <p class=""description-text"">
                                                    Cảm ơn bạn đã tham gia cộng đồng HeartToHeart! Chúng tôi rất vui mừng khi có bạn đồng hành.
                                                    Để hoàn tất đăng ký và bắt đầu hành trình cùng chúng tôi, vui lòng xác nhận địa chỉ email của bạn.
                                                </p>
                                                <div class=""button-container"">
                                                    <a href='{confirmationLink}' class=""confirm-button"">✨ Xác Nhận Địa Chỉ Email</a>
                                                </div>
                                                <div class=""decorative-element"">
                                                    <span class=""dots""></span>
                                                    <span class=""dots""></span>
                                                    <span class=""dots""></span>
                                                </div>
                                                <div class=""link-section"">
                                                    <p class=""link-text"">Nếu nút bên trên không hoạt động, vui lòng sao chép và dán liên kết này vào trình duyệt của bạn:</p>
                                                    <p class=""link-url"">{confirmationLink}</p>
                                                </div>
                                                <div class=""expiry-notice"">
                                                    <strong>⏰ Quan trọng:</strong> Liên kết xác nhận này sẽ hết hạn sau 24 giờ để đảm bảo an toàn.
                                                </div>
                                            </div>
                                            <div class=""footer-section"">
                                                <p class=""footer-text"">Nếu bạn không tạo tài khoản với HeartToHeart, vui lòng bỏ qua email này hoặc liên hệ đội ngũ hỗ trợ của chúng tôi.</p>
                                            </div>
                                        </div>
                                    </div>
                                </body>
                                </html>";

                return await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email confirmation to {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string firstName, string resetToken)
        {
            try
            {
                var subject = "Reset Your Password - HeartToHeart";
                var frontendUrl = _configuration["AppSettings:FrontendUrl"];
                var resetLink = $"{frontendUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(email)}";

                var body = $@"
                                <!DOCTYPE html>
                                <html lang=""vi"">
                                <head>
                                    <meta charset=""UTF-8"">
                                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                    <title>Đặt Lại Mật Khẩu - HeartToHeart</title>
                                    <style>
                                        @import url('https://fonts.googleapis.com/css2?family=Poppins:wght@300;400;600;700&display=swap');
                                        .gradient-bg {{ background: linear-gradient(135deg, #FFFFFF 0%, #FADEDA 50%, #ACB9E7 100%); min-height: 100vh; padding: 40px 20px; }}
                                        .email-container {{ max-width: 600px; margin: 0 auto; background: rgba(255, 255, 255, 0.95); border-radius: 20px; box-shadow: 0 20px 40px rgba(172, 185, 231, 0.3); overflow: hidden; backdrop-filter: blur(10px); }}
                                        .header-section {{ background: linear-gradient(90deg, #ACB9E7, #FADEDA); padding: 40px 30px; text-align: center; position: relative; }}
                                        .header-section::before {{ content: ''; position: absolute; top: 0; left: 0; right: 0; bottom: 0; background: linear-gradient(45deg, rgba(250, 222, 218, 0.1), rgba(172, 185, 231, 0.1)); z-index: 1; }}
                                        .header-content {{ position: relative; z-index: 2; }}
                                        .logo {{ width: 80px; height: 80px; background: linear-gradient(135deg, #FFFFFF, #FADEDA); border-radius: 50%; margin: 0 auto 20px; display: flex; align-items: center; justify-content: center; box-shadow: 0 10px 30px rgba(172, 185, 231, 0.4); }}
                                        .lock-icon {{ font-size: 36px; color: #ACB9E7; }}
                                        .main-title {{ color: #4a4a4a; font-size: 28px; font-weight: 700; margin: 0; text-shadow: 0 2px 4px rgba(172, 185, 231, 0.3); }}
                                        .content-section {{ padding: 40px 30px; color: #4a4a4a; }}
                                        .welcome-text {{ font-size: 18px; font-weight: 600; color: #6b6b6b; margin-bottom: 20px; text-align: center; }}
                                        .description-text {{ font-size: 16px; line-height: 1.8; margin-bottom: 30px; text-align: center; color: #6b6b6b; }}
                                        .button-container {{ text-align: center; margin: 40px 0; }}
                                        .reset-button {{ background: linear-gradient(135deg, #e74c3c, #FADEDA); color: #ffffff; padding: 16px 40px; text-decoration: none; border-radius: 50px; display: inline-block; font-weight: 600; font-size: 16px; box-shadow: 0 10px 30px rgba(231, 76, 60, 0.4); transition: all 0.3s ease; border: 2px solid transparent; }}
                                        .reset-button:hover {{ transform: translateY(-2px); box-shadow: 0 15px 40px rgba(231, 76, 60, 0.6); }}
                                        .link-section {{ background: linear-gradient(135deg, rgba(255, 255, 255, 0.8), rgba(250, 222, 218, 0.3)); padding: 20px; border-radius: 15px; margin: 20px 0; border: 1px solid rgba(172, 185, 231, 0.2); }}
                                        .link-text {{ font-size: 14px; color: #6b6b6b; margin-bottom: 10px; }}
                                        .link-url {{ word-break: break-word; color: #e74c3c; font-size: 14px; font-weight: 500; }}
                                        .expiry-notice {{ background: linear-gradient(135deg, rgba(231, 76, 60, 0.1), rgba(172, 185, 231, 0.2)); padding: 15px; border-radius: 10px; border-left: 4px solid #e74c3c; margin: 20px 0; font-size: 14px; color: #6b6b6b; }}
                                        .security-notice {{ background: linear-gradient(135deg, rgba(250, 222, 218, 0.4), rgba(172, 185, 231, 0.3)); padding: 20px; border-radius: 15px; margin: 20px 0; border-left: 4px solid #FADEDA; font-size: 15px; color: #4a4a4a; font-weight: 600; }}
                                        .footer-section {{ background: linear-gradient(135deg, rgba(172, 185, 231, 0.1), rgba(250, 222, 218, 0.1)); padding: 20px 30px; text-align: center; border-top: 1px solid rgba(172, 185, 231, 0.2); }}
                                        .footer-text {{ font-size: 12px; color: #999; margin: 0; }}
                                        .decorative-element {{ text-align: center; margin: 20px 0; }}
                                        .dots {{ display: inline-block; width: 8px; height: 8px; background: #FADEDA; border-radius: 50%; margin: 0 5px; }}
                                        .dots:nth-child(2) {{ background: #e74c3c; }}
                                        .dots:nth-child(3) {{ background: #ACB9E7; }}
                                    </style>
                                </head>

                                <body style='font-family: ""Poppins"", Arial, sans-serif; margin: 0; padding: 0;'>
                                    <div class=""gradient-bg"">
                                        <div class=""email-container"">
                                            <div class=""header-section"">
                                                <div class=""header-content"">
                                                    <div class=""logo"">
                                                        <span class=""lock-icon"">🔐</span>
                                                    </div>
                                                    <h1 class=""main-title"">Đặt Lại Mật Khẩu</h1>
                                                </div>
                                            </div>
                                            <div class=""content-section"">
                                                <p class=""welcome-text"">Xin chào {firstName},</p>
                                                <p class=""description-text"">
                                                    Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu cho tài khoản HeartToHeart của bạn.
                                                    Vui lòng nhấp vào nút bên dưới để đặt lại mật khẩu:
                                                </p>
                                                <div class=""button-container"">
                                                    <a href='{resetLink}' class=""reset-button"">🔑 Đặt Lại Mật Khẩu</a>
                                                </div>
                                                <div class=""decorative-element"">
                                                    <span class=""dots""></span>
                                                    <span class=""dots""></span>
                                                    <span class=""dots""></span>
                                                </div>
                                                <div class=""link-section"">
                                                    <p class=""link-text"">Nếu nút bên trên không hoạt động, vui lòng sao chép và dán liên kết này vào trình duyệt của bạn:</p>
                                                    <p class=""link-url"">{resetLink}</p>
                                                </div>
                                                <div class=""expiry-notice"">
                                                    <strong>⏰ Quan trọng:</strong> Liên kết này sẽ hết hạn sau 1 giờ vì lý do bảo mật.
                                                </div>
                                                <div class=""security-notice"">
                                                    <strong>🛡️ Lưu ý bảo mật:</strong> Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này. Mật khẩu của bạn sẽ không thay đổi.
                                                </div>
                                            </div>
                                            <div class=""footer-section"">
                                                <p class=""footer-text"">
                                                    Vì lý do bảo mật, liên kết này chỉ hoạt động một lần và sẽ hết hạn sau 1 giờ.
                                                </p>
                                            </div>
                                        </div>
                                    </div>
                                </body>
                                </html>";

                return await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string email, string firstName)
        {
            try
            {
                var subject = "Chào Mừng Đến Với HeartToHeart!";
                var frontendUrl = _configuration["AppSettings:FrontendUrl"];

                var body = $@"
                                <!DOCTYPE html>
                                <html lang=""vi"">
                                <head>
                                    <meta charset=""UTF-8"">
                                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                    <title>Chào Mừng - HeartToHeart</title>
                                    <style>
                                        @import url('https://fonts.googleapis.com/css2?family=Poppins:wght@300;400;600;700&display=swap');
                                        .gradient-bg {{ background: linear-gradient(135deg, #FFFFFF 0%, #FADEDA 50%, #ACB9E7 100%); min-height: 100vh; padding: 40px 20px; }}
                                        .email-container {{ max-width: 650px; margin: 0 auto; background: rgba(255, 255, 255, 0.95); border-radius: 20px; box-shadow: 0 20px 40px rgba(172, 185, 231, 0.3); overflow: hidden; backdrop-filter: blur(10px); }}
                                        .header-section {{ background: linear-gradient(90deg, #ACB9E7, #FADEDA); padding: 50px 30px; text-align: center; position: relative; }}
                                        .header-section::before {{ content: ''; position: absolute; top: 0; left: 0; right: 0; bottom: 0; background: linear-gradient(45deg, rgba(250, 222, 218, 0.1), rgba(172, 185, 231, 0.1)); z-index: 1; }}
                                        .header-content {{ position: relative; z-index: 2; }}
                                        .logo {{ width: 100px; height: 100px; background: linear-gradient(135deg, #FFFFFF, #FADEDA); border-radius: 50%; margin: 0 auto 25px; display: flex; align-items: center; justify-content: center; box-shadow: 0 15px 40px rgba(172, 185, 231, 0.5); }}
                                        .welcome-icon {{ font-size: 45px; color: #ACB9E7; }}
                                        .main-title {{ color: #4a4a4a; font-size: 32px; font-weight: 700; margin: 0 0 15px 0; text-shadow: 0 2px 4px rgba(172, 185, 231, 0.3); }}
                                        .subtitle {{ color: #6b6b6b; font-size: 16px; font-weight: 400; margin: 0; opacity: 0.9; }}
                                        .content-section {{ padding: 40px 35px; color: #4a4a4a; }}
                                        .welcome-message {{ background: linear-gradient(135deg, rgba(250, 222, 218, 0.3), rgba(172, 185, 231, 0.2)); padding: 25px; border-radius: 15px; margin-bottom: 35px; text-align: center; border-left: 4px solid #FADEDA; }}
                                        .welcome-text {{ font-size: 16px; line-height: 1.7; color: #5a5a5a; margin: 0; }}
                                        .section-title {{ font-size: 22px; font-weight: 700; color: #4a4a4a; margin-bottom: 20px; text-align: center; position: relative; }}
                                        .section-title::after {{ content: ''; position: absolute; bottom: -8px; left: 50%; transform: translateX(-50%); width: 60px; height: 3px; background: linear-gradient(90deg, #FADEDA, #ACB9E7); border-radius: 2px; }}
                                        .features-grid {{ display: grid; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap: 20px; margin: 25px 0; }}
                                        .feature-card {{ background: linear-gradient(135deg, rgba(255, 255, 255, 0.9), rgba(250, 222, 218, 0.2)); padding: 20px; border-radius: 15px; border: 1px solid rgba(172, 185, 231, 0.2); transition: transform 0.3s ease, box-shadow 0.3s ease; }}
                                        .feature-card:hover {{ transform: translateY(-5px); box-shadow: 0 15px 30px rgba(172, 185, 231, 0.3); }}
                                        .feature-item {{ display: flex; align-items: center; margin-bottom: 15px; font-size: 15px; line-height: 1.6; color: #5a5a5a; }}
                                        .feature-item:last-child {{ margin-bottom: 0; }}
                                        .feature-icon {{ font-size: 18px; margin-right: 12px; width: 25px; flex-shrink: 0; }}
                                        .premium-section {{ background: linear-gradient(135deg, rgba(172, 185, 231, 0.1), rgba(250, 222, 218, 0.2)); padding: 30px; border-radius: 20px; margin: 35px 0; border: 2px solid rgba(172, 185, 231, 0.3); position: relative; overflow: hidden; }}
                                        .premium-section::before {{ content: '✨'; position: absolute; top: -10px; right: -10px; font-size: 60px; opacity: 0.1; transform: rotate(15deg); }}
                                        .premium-title {{ font-size: 20px; font-weight: 700; color: #4a4a4a; margin-bottom: 15px; text-align: center; }}
                                        .premium-description {{ font-size: 15px; color: #6b6b6b; text-align: center; margin-bottom: 20px; }}
                                        .premium-item {{ display: flex; align-items: center; font-size: 15px; color: #5a5a5a; font-weight: 500; }}
                                        .premium-icon {{ font-size: 16px; margin-right: 12px; width: 25px; flex-shrink: 0; }}
                                        .button-container {{ text-align: center; margin: 40px 0; }}
                                        .get-started-button {{ background: linear-gradient(135deg, #ACB9E7, #FADEDA); color: #4a4a4a; padding: 18px 45px; text-decoration: none; border-radius: 50px; display: inline-block; font-weight: 700; font-size: 16px; box-shadow: 0 10px 30px rgba(172, 185, 231, 0.4); transition: all 0.3s ease; border: 2px solid transparent; text-transform: uppercase; letter-spacing: 1px; }}
                                        .get-started-button:hover {{ transform: translateY(-3px); box-shadow: 0 20px 40px rgba(172, 185, 231, 0.6); }}
                                        .decorative-element {{ text-align: center; margin: 30px 0; }}
                                        .celebration-icons {{ font-size: 24px; margin: 0 8px; opacity: 0.7; }}
                                        .footer-section {{ background: linear-gradient(135deg, rgba(172, 185, 231, 0.1), rgba(250, 222, 218, 0.1)); padding: 25px 35px; text-align: center; border-top: 1px solid rgba(172, 185, 231, 0.2); }}
                                        .footer-text {{ font-size: 13px; color: #999; margin: 0; line-height: 1.5; }}
                                        .support-highlight {{ color: #ACB9E7; font-weight: 600; }}
                                    </style>
                                </head>

                                <body style='font-family: ""Poppins"", Arial, sans-serif; margin: 0; padding: 0;'>
                                                                    <div class=""gradient-bg"">
                                                                        <div class=""email-container"">
                                                                            <div class=""header-section"">
                                                                                <div class=""header-content"">
                                                                                    <div class=""logo"">
                                                                                        <span class=""welcome-icon"">🎉</span>
                                                                                    </div>
                                                                                    <h1 class=""main-title"">Chào Mừng Đến Với HeartToHeart!</h1>
                                                                                    <p class=""subtitle"">Xin chào {firstName}, chúng tôi rất vui mừng có bạn tham gia</p>
                                                                                </div>
                                                                            </div>
                                                                            <div class=""content-section"">
                                                                                <div class=""welcome-message"">
                                                                                    <p class=""welcome-text"">
                                                                                        Tài khoản của bạn đã được xác minh thành công! Chúng tôi rất phấn khích khi bạn tham gia cộng đồng chăm sóc sức khỏe tinh thần của chúng tôi.
                                                                                    </p>
                                                                                </div>
                                                                                <h2 class=""section-title"">Những gì bạn có thể làm ngay bây giờ</h2>
                                                                                <div class=""features-grid"">
                                                                                    <div class=""feature-card"">
                                                                                        <div class=""feature-item""><span class=""feature-icon"">📝</span><span>Tạo bài viết ẩn danh và kết nối với mọi người</span></div>
                                                                                        <div class=""feature-item""><span class=""feature-icon"">🤖</span><span>Trò chuyện với trợ lý AI chăm sóc sức khỏe tinh thần</span></div>
                                                                                    </div>
                                                                                    <div class=""feature-card"">
                                                                                        <div class=""feature-item""><span class=""feature-icon"">📖</span><span>Viết nhật ký cá nhân để theo dõi suy nghĩ của bạn</span></div>
                                                                                        <div class=""feature-item""><span class=""feature-icon"">💚</span><span>Theo dõi tình trạng cảm xúc và tinh thần của bạn</span></div>
                                                                                    </div>
                                                                                </div>
                                                                                <div class=""premium-section"">
                                                                                    <h3 class=""premium-title"">🌟 Sẵn sàng nâng cấp?</h3>
                                                                                    <p class=""premium-description"">Cân nhắc gói thành viên Premium để có thêm các tính năng như:</p>
                                                                                    <div class=""premium-features"">
                                                                                        <div class=""premium-item""><span class=""premium-icon"">👨‍⚕️</span><span>Trò chuyện trực tiếp với các chuyên gia tư vấn tâm lý</span></div>
                                                                                        <div class=""premium-item""><span class=""premium-icon"">📚</span><span>Truy cập tài nguyên sức khỏe tinh thần cao cấp</span></div>
                                                                                        <div class=""premium-item""><span class=""premium-icon"">⚡</span><span>Hỗ trợ ưu tiên và đặt lịch hẹn nhanh chóng</span></div>
                                                                                    </div>
                                                                                </div>
                                                                                <div class=""decorative-element"">
                                                                                    <span class=""celebration-icons"">🎊</span>
                                                                                    <span class=""celebration-icons"">💖</span>
                                                                                    <span class=""celebration-icons"">🎊</span>
                                                                                </div>
                                                                                <div class=""button-container"">
                                                                                    <a href='{frontendUrl}' class=""get-started-button"">🚀 Bắt Đầu Ngay</a>
                                                                                </div>
                                                                            </div>
                                                                            <div class=""footer-section"">
                                                                                <p class=""footer-text"">
                                                                                    Bạn cần hỗ trợ? <span class=""support-highlight"">Liên hệ đội ngũ hỗ trợ của chúng tôi bất cứ lúc nào.</span><br>
                                                                                    Chúng tôi luôn sẵn sàng đồng hành cùng bạn trong hành trình chăm sóc sức khỏe tinh thần.
                                                                                </p>
                                                                            </div>
                                                                        </div>
                                                                    </div>
                                                                </body>
                                                                </html>";

                return await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to {Email}", email);
                return false;
            }
        }
        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                // Validate SMTP configuration before sending
                if (!IsSmtpConfigured())
                {
                    _logger.LogError("SMTP is not properly configured. Missing required settings.");
                    return false;
                }

                var fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@hearttoheart.com";
                var fromName = _configuration["EmailSettings:FromName"] ?? "HeartToHeart";

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);

                await _smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", to);
                return true;
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "SMTP error occurred while sending email to {Email}. Error: {Error}", to, ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", to);
                return false;
            }
        }

        private SmtpClient ConfigureSmtpClient()
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            var smtpUsername = emailSettings["SmtpUsername"] ?? "";
            var smtpPassword = emailSettings["SmtpPassword"] ?? "";

            if (string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
            {
                _logger.LogWarning("SMTP credentials are not configured. Email sending will fail.");
            }

            return new SmtpClient
            {
                Host = emailSettings["SmtpHost"] ?? "smtp.gmail.com",
                Port = int.Parse(emailSettings["SmtpPort"] ?? "587"),
                EnableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true"),
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 30000 // 30 seconds timeout
            };
        }

        private bool IsSmtpConfigured()
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var smtpUsername = emailSettings["SmtpUsername"];
            var smtpPassword = emailSettings["SmtpPassword"];
            var fromEmail = emailSettings["FromEmail"];

            return !string.IsNullOrEmpty(smtpUsername) &&
                   !string.IsNullOrEmpty(smtpPassword) &&
                   !string.IsNullOrEmpty(fromEmail);
        }

        public void Dispose()
        {
            _smtpClient?.Dispose();
        }
    }
}