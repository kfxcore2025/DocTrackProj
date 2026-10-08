namespace DocTrackProj.Models
{
    public class ManageViewModel
    {
        public List<User> Users { get; set; } = new();
        public List<Office> Offices { get; set; } = new();
    }
}