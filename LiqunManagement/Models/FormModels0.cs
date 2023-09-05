using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace LiqunManagement.Models
{
    public partial class FormModels0 : DbContext
    {
        public FormModels0()
            : base("name=FormModels0")
        {
        }

        public virtual DbSet<Bank> Bank { get; set; }
        public virtual DbSet<HomeObject> HomeObject { get; set; }
        public virtual DbSet<LandLord> LandLord { get; set; }
        public virtual DbSet<ObjectForm> ObjectForm { get; set; }
        public virtual DbSet<Region> Region { get; set; }
        public virtual DbSet<Tenant> Tenant { get; set; }

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
                .Property(e => e.FormID)
                .IsUnicode(false);

            modelBuilder.Entity<HomeObject>()
                .Property(e => e.CaseId)
                .IsUnicode(false);

            modelBuilder.Entity<HomeObject>()
                .Property(e => e.roomamount)
                .IsUnicode(false);

            modelBuilder.Entity<HomeObject>()
                .Property(e => e.havepark)
                .IsUnicode(false);

            modelBuilder.Entity<HomeObject>()
                .Property(e => e.parkfloor)
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
                .Property(e => e.FormID)
                .IsUnicode(false);

            modelBuilder.Entity<LandLord>()
                .Property(e => e.IDNumber)
                .IsUnicode(false);

            modelBuilder.Entity<LandLord>()
                .Property(e => e.Phone)
                .IsUnicode(false);

            modelBuilder.Entity<LandLord>()
                .Property(e => e.BankNo)
                .IsUnicode(false);

            modelBuilder.Entity<LandLord>()
                .Property(e => e.BrancheNo)
                .IsUnicode(false);

            modelBuilder.Entity<LandLord>()
                .Property(e => e.BankAccount)
                .IsUnicode(false);

            modelBuilder.Entity<LandLord>()
                .Property(e => e.RoadCode)
                .IsUnicode(false);

            modelBuilder.Entity<LandLord>()
                .Property(e => e.RoadCodeContact)
                .IsUnicode(false);

            modelBuilder.Entity<LandLord>()
                .Property(e => e.MemberArray)
                .IsUnicode(false);

            modelBuilder.Entity<ObjectForm>()
                .Property(e => e.FormID)
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

            modelBuilder.Entity<Tenant>()
                .Property(e => e.FormID)
                .IsUnicode(false);

            modelBuilder.Entity<Tenant>()
                .Property(e => e.vulnerablefile_Alias)
                .IsUnicode(false);

            modelBuilder.Entity<Tenant>()
                .Property(e => e.sheetfile_Alias)
                .IsUnicode(false);

            modelBuilder.Entity<Tenant>()
                .Property(e => e.IDNumber)
                .IsUnicode(false);

            modelBuilder.Entity<Tenant>()
                .Property(e => e.Phone)
                .IsUnicode(false);

            modelBuilder.Entity<Tenant>()
                .Property(e => e.RoadCode)
                .IsUnicode(false);

            modelBuilder.Entity<Tenant>()
                .Property(e => e.RoadCodeContact)
                .IsUnicode(false);

            modelBuilder.Entity<Tenant>()
                .Property(e => e.accountNo)
                .IsUnicode(false);

            modelBuilder.Entity<Tenant>()
                .Property(e => e.BankNo)
                .IsUnicode(false);

            modelBuilder.Entity<Tenant>()
                .Property(e => e.BrancheNo)
                .IsUnicode(false);

            modelBuilder.Entity<Tenant>()
                .Property(e => e.BankAccount)
                .IsUnicode(false);

            modelBuilder.Entity<Tenant>()
                .Property(e => e.MemberArray)
                .IsUnicode(false);
        }
    }
}
