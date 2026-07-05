namespace ACI.DTO.ReqDTO.Email
{
    public class EmailSendReqDTO
    {
        public List<string>? To_Address { get; set; }
        public List<string>? CC_Address { get; set; }
        public List<string>? BCC_Address { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string LogBody { get; set; } = string.Empty;
        public List<IFormFile>? File { get; set; }
    }
}
