using System.Data.Entity;

namespace MindLinc.Model
{
    public class PatientDbContext : DbContext
    {
        public PatientDbContext()
            : base("name=PatientDbContext")
        {
        }

        public virtual DbSet<Patient> patients { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Patient>()
                .Property(e => e.id)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Patient>()
                .Property(e => e.family_name)
                .IsFixedLength();

            modelBuilder.Entity<Patient>()
                .Property(e => e.given_name)
                .IsFixedLength();

            modelBuilder.Entity<Patient>()
                .Property(e => e.gender)
                .IsFixedLength();

            modelBuilder.Entity<Patient>()
                .Property(e => e.marital_status)
                .IsFixedLength();

            modelBuilder.Entity<Patient>()
                .Property(e => e.address)
                .IsFixedLength();

            modelBuilder.Entity<Patient>()
                .Property(e => e.telecom)
                .IsFixedLength();

            modelBuilder.Entity<Patient>()
                .Property(e => e.language)
                .IsFixedLength();

            modelBuilder.Entity<Patient>()
                .Property(e => e.managing_organization)
                .IsFixedLength();
        }
    }
}
