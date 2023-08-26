using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace LiqunManagement.Models
{
    public partial class FormModels : DbContext
    {
        public FormModels()
            : base("name=FormModels")
        {
        }

        public virtual DbSet<Bank> Bank { get; set; }
        public virtual DbSet<HomeObject> HomeObject { get; set; }
        public virtual DbSet<LandLord> LandLord { get; set; }
        public virtual DbSet<ObjectForm> ObjectForm { get; set; }
        public virtual DbSet<Region> Region { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bank>()
                .Property(e => e.BankCode)
                .IsUnicode(false);

            modelBuilder.Entity<Bank>()
                .Property(e => e.BranchCode)
                .IsUnicode(false);

            modelBuilder.Entity<Bank>()
                .Property(e => e.CodeMinlength)
                .IsUnicode(false);

            modelBuilder.Entity<Bank>()
                .Property(e => e.CodeMaxlength)
                .IsUnicode(false);

            modelBuilder.Entity<HomeObject>()
                .Property(e => e.FormId)
                .IsUnicode(false);

            modelBuilder.Entity<HomeObject>()
                .Property(e => e.CaseId)
                .IsUnicode(false);

            modelBuilder.Entity<HomeObject>()
                .Property(e => e.taxfile_alias)
                .IsUnicode(false);

            modelBuilder.Entity<HomeObject>()
                .Property(e => e.roomamount)
                .IsUnicode(false);

            modelBuilder.Entity<HomeObject>()
                .Property(e => e.havepark)
                .IsUnicode(false);

            modelBuilder.Entity<HomeObject>()
                .Property(e => e.carparkfloor)
                .IsUnicode(false);

            modelBuilder.Entity<HomeObject>()
                .Property(e => e.carpositionnumber)
                .IsUnicode(false);

            modelBuilder.Entity<HomeObject>()
                .Property(e => e.scooterparkfloor)
                .IsUnicode(false);

            modelBuilder.Entity<HomeObject>()
                .Property(e => e.scooterpositionnumber)
                .IsUnicode(false);

            modelBuilder.Entity<HomeObject>()
                .Property(e => e.Accessory)
                .IsUnicode(false);

            modelBuilder.Entity<LandLord>()
                .Property(e => e.FormId)
                .IsUnicode(false);

            modelBuilder.Entity<LandLord>()
                .Property(e => e.Lordtype)
                .IsUnicode(false);

            modelBuilder.Entity<LandLord>()
                .Property(e => e.IDNumber)
                .IsUnicode(false);

            modelBuilder.Entity<LandLord>()
                .Property(e => e.PhoneNumber)
                .IsUnicode(false);

            modelBuilder.Entity<LandLord>()
                .Property(e => e.RoadCode)
                .IsUnicode(false);

            modelBuilder.Entity<LandLord>()
                .Property(e => e.RoadCodeContact)
                .IsUnicode(false);

            modelBuilder.Entity<ObjectForm>()
                .Property(e => e.FormId)
                .IsUnicode(false);

            modelBuilder.Entity<ObjectForm>()
                .Property(e => e.CreateAccount)
                .IsUnicode(false);

            modelBuilder.Entity<ObjectForm>()
                .Property(e => e.UpdateAccount)
                .IsUnicode(false);

            modelBuilder.Entity<ObjectForm>()
                .Property(e => e.ProcessAccount)
                .IsUnicode(false);

            modelBuilder.Entity<Region>()
                .Property(e => e.CityCode)
                .IsUnicode(false);

            modelBuilder.Entity<Region>()
                .Property(e => e.DistrictCode)
                .IsUnicode(false);

            modelBuilder.Entity<Region>()
                .Property(e => e.RoadCode)
                .IsUnicode(false);
        }
    }
}
