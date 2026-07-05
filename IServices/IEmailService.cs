using ACI.DTO.ReqDTO.Email;
using ACI.DTO.ResDTO;

namespace ACI.Service.IService
{
    public interface IEmailService
    {
        Task<bool> MailSending(EmailSendReqDTO Mv);
        Task<bool> SaveFile(IFormFile File, string PathSave);
        Task<EmailHistoryResDTO> GetEmailHistoryAsync(int pageNumber, int pageSize);
    }
}
