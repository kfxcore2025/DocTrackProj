using Microsoft.EntityFrameworkCore;
using DocTrackProj.Models;

namespace DocTrackProj.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Document> Documents { get; set; }

        public DbSet<UserDocument> UserDocuments { get; set; }

        public DbSet<AdminDocument> AdminDocuments { get; set; }

        public DbSet<Office> Offices { get; set; }

        public DbSet<EmailVerification> EmailVerifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================
            // USER → OFFICE
            // =========================
            modelBuilder.Entity<User>()
        .HasOne(u => u.Office)
        .WithMany()
        .HasForeignKey(u => u.OfficeId)
        .OnDelete(DeleteBehavior.SetNull);

            // =========================
            // EXISTING DOCUMENT → FROM OFFICE
            // =========================
            modelBuilder.Entity<Document>()
        .HasOne(d => d.FromOffice)
        .WithMany()
        .HasForeignKey(d => d.FromOfficeId)
        .OnDelete(DeleteBehavior.NoAction);

            // =========================
            // EXISTING DOCUMENT → TO OFFICE
            // =========================
            modelBuilder.Entity<Document>()
        .HasOne(d => d.ToOffice)
        .WithMany()
        .HasForeignKey(d => d.ToOfficeId)
        .OnDelete(DeleteBehavior.NoAction);

            // =========================
            // USER DOCUMENT → FROM OFFICE
            // =========================
            modelBuilder.Entity<UserDocument>()
        .HasOne(d => d.FromOffice)
        .WithMany()
        .HasForeignKey(d => d.FromOfficeId)
        .OnDelete(DeleteBehavior.NoAction);

            // =========================
            // USER DOCUMENT → TO OFFICE
            // =========================
            modelBuilder.Entity<UserDocument>()
        .HasOne(d => d.ToOffice)
        .WithMany()
        .HasForeignKey(d => d.ToOfficeId)
        .OnDelete(DeleteBehavior.NoAction);

            // Index used to connect the matching UserDocument
            // and AdminDocument records
            modelBuilder.Entity<UserDocument>()
        .HasIndex(d => d.TransferKey)
        .IsUnique(false);

            // =========================
            // ADMIN DOCUMENT → FROM OFFICE
            // =========================
            modelBuilder.Entity<AdminDocument>()
        .HasOne(d => d.FromOffice)
        .WithMany()
        .HasForeignKey(d => d.FromOfficeId)
        .OnDelete(DeleteBehavior.NoAction);

            // =========================
            // ADMIN DOCUMENT → TO OFFICE
            // =========================
            modelBuilder.Entity<AdminDocument>()
        .HasOne(d => d.ToOffice)
        .WithMany()
        .HasForeignKey(d => d.ToOfficeId)
        .OnDelete(DeleteBehavior.NoAction);

            // Index used to connect the matching AdminDocument
            // and UserDocument records
            modelBuilder.Entity<AdminDocument>()
        .HasIndex(d => d.TransferKey)
        .IsUnique(false);

            // =========================
            // SEED OFFICES
            // =========================
            modelBuilder.Entity<Office>().HasData(
                new Office
                {
                    Id = 1,
                    OfficeName = "Accounting",
                    RoomNumber = "104"
                },
                new Office
                {
                    Id = 2,
                    OfficeName = "Accreditation and Quality Assurance",
                    RoomNumber = "135"
                },
                new Office
                {
                    Id = 3,
                    OfficeName = "Admin Office",
                    RoomNumber = "102"
                },
                new Office
                {
                    Id = 4,
                    OfficeName = "Assistant Campus Director's/Dean of Instruction",
                    RoomNumber = "108"
                },
                new Office
                {
                    Id = 5,
                    OfficeName = "BAC Office",
                    RoomNumber = "109"
                },
                new Office
                {
                    Id = 6,
                    OfficeName = "BSFI Department",
                    RoomNumber = "116"
                },
                new Office
                {
                    Id = 7,
                    OfficeName = "BSIT Department",
                    RoomNumber = "113"
                },
                new Office
                {
                    Id = 8,
                    OfficeName = "Campus Director",
                    RoomNumber = "101"
                },
                new Office
                {
                    Id = 9,
                    OfficeName = "Campus Secretary",
                    RoomNumber = "103"
                },
                new Office
                {
                    Id = 10,
                    OfficeName = "Canteen",
                    RoomNumber = "139"
                },
                new Office
                {
                    Id = 11,
                    OfficeName = "Cashier",
                    RoomNumber = "105"
                },
                new Office
                {
                    Id = 12,
                    OfficeName = "Civil Security Services",
                    RoomNumber = "137"
                },
                new Office
                {
                    Id = 13,
                    OfficeName = "Clinic",
                    RoomNumber = "120"
                },
                new Office
                {
                    Id = 14,
                    OfficeName = "COA",
                    RoomNumber = "106"
                },
                new Office
                {
                    Id = 15,
                    OfficeName = "College of Education",
                    RoomNumber = "114"
                },
                new Office
                {
                    Id = 16,
                    OfficeName = "College of Technology & Engineering",
                    RoomNumber = "115"
                },
                new Office
                {
                    Id = 17,
                    OfficeName = "DOST",
                    RoomNumber = "128"
                },
                new Office
                {
                    Id = 18,
                    OfficeName = "Extension Services",
                    RoomNumber = "129"
                },
                new Office
                {
                    Id = 19,
                    OfficeName = "Gender and Development",
                    RoomNumber = "121"
                },
                new Office
                {
                    Id = 20,
                    OfficeName = "Graduate Studies",
                    RoomNumber = "119"
                },
                new Office
                {
                    Id = 21,
                    OfficeName = "Guidance Counselor",
                    RoomNumber = "133"
                },
                new Office
                {
                    Id = 22,
                    OfficeName = "HM Department",
                    RoomNumber = "117"
                },
                new Office
                {
                    Id = 23,
                    OfficeName = "Hometel",
                    RoomNumber = "126"
                },
                new Office
                {
                    Id = 24,
                    OfficeName = "HRMO",
                    RoomNumber = "107"
                },
                new Office
                {
                    Id = 25,
                    OfficeName = "IA/SIAP (MAAM MANTOS)",
                    RoomNumber = "138"
                },
                new Office
                {
                    Id = 26,
                    OfficeName = "ICPA",
                    RoomNumber = "134"
                },
                new Office
                {
                    Id = 27,
                    OfficeName = "Library",
                    RoomNumber = "124"
                },
                new Office
                {
                    Id = 28,
                    OfficeName = "Maintenance/Carpentry",
                    RoomNumber = "122"
                },
                new Office
                {
                    Id = 29,
                    OfficeName = "MIS Assessment",
                    RoomNumber = "111"
                },
                new Office
                {
                    Id = 30,
                    OfficeName = "MIS Office",
                    RoomNumber = "110"
                },
                new Office
                {
                    Id = 31,
                    OfficeName = "NSTP",
                    RoomNumber = "130"
                },
                new Office
                {
                    Id = 32,
                    OfficeName = "PACD/Guardhouse",
                    RoomNumber = "127"
                },
                new Office
                {
                    Id = 33,
                    OfficeName = "Production Department",
                    RoomNumber = "132"
                },
                new Office
                {
                    Id = 34,
                    OfficeName = "Research and Development",
                    RoomNumber = "113"
                },
                new Office
                {
                    Id = 35,
                    OfficeName = "Resource Generation",
                    RoomNumber = "136"
                },
                new Office
                {
                    Id = 36,
                    OfficeName = "Smart Room",
                    RoomNumber = "140"
                },
                new Office
                {
                    Id = 37,
                    OfficeName = "SSG",
                    RoomNumber = "131"
                },
                new Office
                {
                    Id = 38,
                    OfficeName = "Students Affairs Services",
                    RoomNumber = "125"
                },
                new Office
                {
                    Id = 39,
                    OfficeName = "Supply Office",
                    RoomNumber = "123"
                },
                new Office
                {
                    Id = 40,
                    OfficeName = "University Registrar",
                    RoomNumber = "112"
                }
            );
        }
    }
}