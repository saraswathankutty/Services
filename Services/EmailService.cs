using System.Net.Mail;
using System.Net;
using ACI.Service.IService;
using ACI.DTO.ReqDTO.Email;
using ACI.Common;
using ACI.IServices.Main.Dapper;
using ACI.Data;
using ACI.Entities;
using ACI.DTO.ResDTO;

namespace ACI.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly IDapperService _dp;
        private readonly AppDbContext _db;
        private readonly ILogger<EmailService> _log;

        public EmailService(IConfiguration config, ILogger<EmailService> log, IDapperService dp, AppDbContext db) 
        {
            _config = config;
            _log = log;
            _dp = dp;
            _db = db;
        }

        public async Task<bool> MailSending(EmailSendReqDTO Mv)
        {
            bool res = false;
            try
            {
                _log.LogInformation("MailSending :: Process :: Start");
                string senderEmail = _config[EmailConstantVariable.EMAIL_NAME];
                string senderPassword = _config[EmailConstantVariable.PASSWORD];
                int Portno = Convert.ToInt16(_config[EmailConstantVariable.PORT]);
                bool SSLs = Convert.ToBoolean(_config[EmailConstantVariable.SSL]);
                MailMessage message = new MailMessage();

                string toAddressesStr = "";
                if(Mv.To_Address != null && Mv.To_Address.Count > 0)
                {
                    toAddressesStr = string.Join(",", Mv.To_Address);
                    foreach (string toAddress in Mv.To_Address)
                    {
                        message.To.Add(new MailAddress(toAddress));
                    }
                }

                if (Mv.CC_Address != null && Mv.CC_Address.Count > 0)
                {
                    foreach (string toAddress in Mv.CC_Address)
                    {
                        message.CC.Add(new MailAddress(toAddress));
                    }
                }

                if (Mv.BCC_Address != null && Mv.BCC_Address.Count > 0)
                {
                    foreach (string toAddress in Mv.BCC_Address)
                    {
                        message.Bcc.Add(new MailAddress(toAddress));
                    }
                }

                message.From = new MailAddress(senderEmail, "Portfolio Contact");
                message.Subject = Mv.Subject;
                message.Body = Mv.Body;
                message.IsBodyHtml = true;

                if (Mv.File != null && Mv.File.Count > 0)
                {
                    foreach (var a in Mv.File)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            await a.CopyToAsync(ms);
                            byte[] fileBytes = ms.ToArray();
                            System.Net.Mail.Attachment attachment = 
                                new System.Net.Mail.Attachment(new MemoryStream(fileBytes), a.FileName, a.ContentType);
                            message.Attachments.Add(attachment);
                        }
                    }
                }

                SmtpClient client = new SmtpClient("smtp.gmail.com")
                {
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = SSLs,
                    Port = Portno,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 30000
                };

                await client.SendMailAsync(message);
                res = true;

                var emailLog = new EmailLog
                {
                    FromAddress = senderEmail,
                    ToAddress = toAddressesStr,
                    Subject = Mv.Subject,
                    MessageBody = string.IsNullOrEmpty(Mv.LogBody) ? Mv.Body : Mv.LogBody,
                    CreatedDate = DateTime.UtcNow,
                    Status = "Sent",
                    ErrorMessage = ""
                };
                _db.EmailLogs.Add(emailLog);
                await _db.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "EmailSending :: Process :: Error");
                
                var emailLog = new EmailLog
                {
                    FromAddress = _config[EmailConstantVariable.EMAIL_NAME],
                    ToAddress = Mv.To_Address != null ? string.Join(",", Mv.To_Address) : "",
                    Subject = Mv.Subject,
                    MessageBody = string.IsNullOrEmpty(Mv.LogBody) ? Mv.Body : Mv.LogBody,
                    CreatedDate = DateTime.UtcNow,
                    Status = "Failed",
                    ErrorMessage = ex.Message
                };
                _db.EmailLogs.Add(emailLog);
                await _db.SaveChangesAsync();
            }
            return await Task.FromResult(res);
        }

        public async Task<EmailHistoryResDTO> GetEmailHistoryAsync(int pageNumber, int pageSize)
        {
            int offset = (pageNumber - 1) * pageSize;
            string query = @"
                SELECT Id, FromAddress, ToAddress, Subject, MessageBody, CreatedDate, Status, ErrorMessage FROM EmailLogs 
                ORDER BY CreatedDate DESC
                OFFSET @Offset ROWS 
                FETCH NEXT @PageSize ROWS ONLY;
            ";

            var logs = await _dp.QueryAsync<EmailLog>(query, new { Offset = offset, PageSize = pageSize });
            
            string countQuery = "SELECT COUNT(Id) FROM EmailLogs;";
            int totalRecords = await _dp.QueryFirstOrDefaultAsync<int>(countQuery);

            return new EmailHistoryResDTO
            {
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Logs = logs
            };
        }
       
        public async Task<bool> SaveFile(IFormFile File, string PathSave)
        {
            bool res = false;
            try
            {
                string fullPath = Path.Combine(PathSave, File.FileName);
                if (!Directory.Exists(PathSave))
                {
                    Directory.CreateDirectory(PathSave);
                }
                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    await File.CopyToAsync(fileStream);
                }
                res = true;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "SaveFile :: Error");
                res = false;
            }
            return await Task.FromResult(res);
        }
    }
}
