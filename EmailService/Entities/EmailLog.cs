using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACI.Entities
{
    [Table("EmailLogs")]
    public class EmailLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string FromAddress { get; set; }
        
        [Required]
        public string ToAddress { get; set; } // comma separated or single
        
        [MaxLength(500)]
        public string Subject { get; set; }
        
        public string MessageBody { get; set; }
        
        [Required]
        public DateTime CreatedDate { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } // e.g. "Sent", "Failed"
        
        public string ErrorMessage { get; set; }
    }
}
