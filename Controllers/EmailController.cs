using ACI.DTO.ReqDTO.Email;
using ACI.DTO.ResDTO;
using ACI.Service.IService;
using ACI.IServices.Main.Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ACI.Entities;

using ACI.IServices.Main.Portfolio;

namespace ACI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly IPortfolioService _portfolioService;

        public EmailController(IEmailService emailService, IPortfolioService portfolioService)
        {
            _emailService = emailService;
            _portfolioService = portfolioService;
        }

        // Common usage endpoint (Requires Auth)
        [HttpPost("SendEmail")]
        public async Task<IActionResult> SendEmail([FromBody] EmailSendReqDTO req)
        {
            try
            {
                bool result = await _emailService.MailSending(req);
                if (result)
                {
                    return Ok(new ResponseDTO(true, "Email sent successfully."));
                }
                return BadRequest(new ResponseDTO(false, "Failed to send email."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO(false, ex.Message));
            }
        }

        // Specific endpoint for the Portfolio Contact Form (No Auth)
        [HttpPost("SendContact")]
        [AllowAnonymous]
        public async Task<IActionResult> SendContact([FromBody] ContactFormReqDTO req)
        {
            try
            {
                bool result = await _portfolioService.SendContactEmailsAsync(req);
                if (result)
                {
                    return Ok(new ResponseDTO(true, "Thank You! Your message has been sent."));
                }
                return BadRequest(new ResponseDTO(false, "Failed to send your message."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO(false, ex.Message));
            }
        }

        [HttpGet("History")]
        public async Task<IActionResult> GetHistory(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var history = await _emailService.GetEmailHistoryAsync(pageNumber, pageSize);
                return Ok(new ResponseDTO(history));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO(false, ex.Message));
            }
        }
    }
}
