using System.ComponentModel.DataAnnotations;

namespace DocTrackProj.Models
{
    public class Office
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string OfficeName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string RoomNumber { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}