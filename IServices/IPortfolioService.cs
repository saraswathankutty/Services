using ACI.DTO.ReqDTO.Email;

namespace ACI.IServices.Main.Portfolio
{
    public interface IPortfolioService
    {
        Task<bool> SendContactEmailsAsync(ContactFormReqDTO req);
    }
}
