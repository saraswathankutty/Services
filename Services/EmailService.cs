using System.Net.Mail;
using System.Net;
using System.Net.Http.Json;
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
                /*===================================================================
                  ORIGINAL SMTP CODE (COMMENTED OUT FOR RENDER HOSTING)
                  ===================================================================
                _log.LogInformation("MailSending :: Process :: Start");
                string senderEmail = _config[EmailConstantVariable.EMAIL_NAME];
                string senderPassword = _config[EmailConstantVariable.PASSWORD];
                int Portno = Convert.ToInt16(_config[EmailConstantVariable.PORT]);
                bool SSLs = Convert.ToBoolean(_config[EmailConstantVariable.SSL]);
                _log.LogInformation($"MailSending :: Configuration Loaded. Sender: {senderEmail}, Port: {Portno}, SSL: {SSLs}");
                MailMessage message = new MailMessage();

                string toAddressesStr = "";
                if(Mv.To_Address != null && Mv.To_Address.Count > 0)
                {
                    toAddressesStr = string.Join(",", Mv.To_Address);
                    foreach (string toAddress in Mv.To_Address)
                    {
                        message.To.Add(new MailAddress(toAddress));
                    }
                    _log.LogInformation($"MailSending :: Added {Mv.To_Address.Count} To Address(es)");
                }
                else
                {
                    _log.LogWarning("MailSending :: No To Address(es) provided");
                }

                if (Mv.CC_Address != null && Mv.CC_Address.Count > 0)
                {
                    foreach (string toAddress in Mv.CC_Address)
                    {
                        message.CC.Add(new MailAddress(toAddress));
                    }
                    _log.LogInformation($"MailSending :: Added {Mv.CC_Address.Count} CC Address(es)");
                }

                if (Mv.BCC_Address != null && Mv.BCC_Address.Count > 0)
                {
                    foreach (string toAddress in Mv.BCC_Address)
                    {
                        message.Bcc.Add(new MailAddress(toAddress));
                    }
                    _log.LogInformation($"MailSending :: Added {Mv.BCC_Address.Count} BCC Address(es)");
                }

                message.From = new MailAddress(senderEmail, "Portfolio Contact");
                message.Subject = Mv.Subject;
                message.Body = Mv.Body;
                message.IsBodyHtml = true;
                _log.LogInformation("MailSending :: Basic message details (Subject, Body) set up");

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
                    _log.LogInformation($"MailSending :: Processed and attached {Mv.File.Count} file(s)");
                }

                SmtpClient client = new SmtpClient("smtp.gmail.com")
                {
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = SSLs,
                    Port = Portno,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 30000
                };

                _log.LogInformation("MailSending :: Attempting to send email via SmtpClient");
                await client.SendMailAsync(message);
                _log.LogInformation("MailSending :: Email successfully sent via SmtpClient");
                res = true;

                _log.LogInformation("MailSending :: Saving email log to database with Sent status");
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
                _log.LogInformation("MailSending :: Process :: End - Success");
                ===================================================================*/

                _log.LogInformation("MailSending :: Forwarding request to Somee API");

                using var httpClient = new HttpClient();
                
                var payload = new 
                {
                    To_Address = Mv.To_Address,
                    CC_Address = Mv.CC_Address,
                    BCC_Address = Mv.BCC_Address,
                    Subject = Mv.Subject,
                    Body = Mv.Body,
                    LogBody = Mv.LogBody
                };

                string apiUrl = _config[EmailConstantVariable.API_URL];
                _log.LogInformation($"MailSending :: Attempting to send request to {apiUrl}");
                
                // If the SendEmail endpoint on Somee requires a JWT token, you will need to add it here
                // e.g., httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "YOUR_TOKEN");

                var response = await httpClient.PostAsJsonAsync(apiUrl, payload);
                
                if (response.IsSuccessStatusCode)
                {
                    _log.LogInformation("MailSending :: Email forwarded successfully to Somee API");
                    res = true;
                }
                else
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    _log.LogError($"MailSending :: Somee API returned status {response.StatusCode}. Body: {responseBody}");
                    throw new Exception($"Somee API failed with status {response.StatusCode}");
                }
                
                return res;
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
