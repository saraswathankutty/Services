using ACI.DTO.ReqDTO.Email;
using ACI.IServices.Main.Portfolio;
using ACI.Service.IService;

namespace ACI.Service
{
    public class PortfolioService : IPortfolioService
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<PortfolioService> _logger;

        public PortfolioService(IEmailService emailService, ILogger<PortfolioService> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<bool> SendContactEmailsAsync(ContactFormReqDTO req)
        {
            try
            {
                // 1. Send email to Me (Portfolio Owner)
                string contactTemplatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "ContactTemplate.html");
                string contactBody = req.Message;
                if (System.IO.File.Exists(contactTemplatePath))
                {
                    string template = await System.IO.File.ReadAllTextAsync(contactTemplatePath);
                    contactBody = template.Replace("{{Name}}", req.Name)
                                   .Replace("{{Email}}", req.Email)
                                   .Replace("{{HrName}}", req.Hr_Name)
                                   .Replace("{{CompanyName}}", req.Company_Name)
                                   .Replace("{{Subject}}", req.Subject)
                                   .Replace("{{Message}}", req.Message);
                }

                var emailToMe = new EmailSendReqDTO
                {
                    To_Address = new List<string> { "saraswathandev@gmail.com" }, // Owner email
                    Subject = string.IsNullOrEmpty(req.Subject) ? $"Portfolio Contact - {req.Name}" : req.Subject,
                    Body = contactBody,
                    LogBody = System.Text.Json.JsonSerializer.Serialize(req)
                };

                bool sentToMe = await _emailService.MailSending(emailToMe);

                // 2. Send Thank You email to the sender (HR / User)
                if (!string.IsNullOrEmpty(req.Email))
                {
                    string thankYouTemplatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "ThankYouTemplate.html");
                    string thankYouBody = $"Thank You, {req.Name}! We have received your message regarding \"{req.Subject}\".";
                    
                    if (System.IO.File.Exists(thankYouTemplatePath))
                    {
                        string template = await System.IO.File.ReadAllTextAsync(thankYouTemplatePath);
                        thankYouBody = template.Replace("{{Name}}", req.Name)
                                               .Replace("{{Subject}}", string.IsNullOrEmpty(req.Subject) ? "Portfolio Inquiry" : req.Subject);
                    }

                    var emailToSender = new EmailSendReqDTO
                    {
                        To_Address = new List<string> { req.Email },
                        Subject = "Thank You for Contacting Me",
                        Body = thankYouBody,
                        LogBody = System.Text.Json.JsonSerializer.Serialize(new { Name = req.Name, Subject = req.Subject, Template = "ThankYouEmail" })
                    };

                    await _emailService.MailSending(emailToSender);
                }

                return sentToMe;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending portfolio contact emails.");
                return false;
            }
        }
    }
}
