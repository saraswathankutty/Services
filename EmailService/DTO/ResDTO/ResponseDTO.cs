namespace ACI.DTO.ResDTO
{
    public class ResponseDTO
    {
        public bool Status { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; } = null;
        public ResponseDTO() {   }
        public ResponseDTO(object data)
        {
            Data = data;
        }
        public ResponseDTO(bool status, string message)
        {
            Status = status;
            Message = message;
        }
    }
}
