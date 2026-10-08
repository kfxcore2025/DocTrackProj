using System;
using System.ComponentModel.DataAnnotations;

namespace DocTrackProj.Models
{
    public class UserDocument
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string TrackingCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string DocumentName { get; set; } = string.Empty;

        public int FromOfficeId { get; set; }
        public Office? FromOffice { get; set; }

        public int ToOfficeId { get; set; }
        public Office? ToOffice { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        public string Reason { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime DateSubmitted { get; set; } = DateTime.Now;

        public DateTime DateUpdated { get; set; } = DateTime.Now;

        // Same value is stored in the matching AdminDocument
        public Guid TransferKey { get; set; } = Guid.NewGuid();
    }
}