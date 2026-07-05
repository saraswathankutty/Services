using ACI.Entities;
using System.Collections.Generic;

namespace ACI.DTO.ResDTO
{
    public class EmailHistoryResDTO
    {
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public IEnumerable<EmailLog> Logs { get; set; } = new List<EmailLog>();
    }
}
