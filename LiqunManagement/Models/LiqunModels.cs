using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace LiqunManagement.Models
{
    public partial class LiqunModels : DbContext
    {
        public LiqunModels()
            : base("name=LiqunModels")
        {
        }

        public virtual DbSet<AllForm> AllForm { get; set; }
        public virtual DbSet<Bank> Bank { get; set; }
        public virtual DbSet<HomeObject> HomeObject { get; set; }
        public virtual DbSet<Region> Region { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AllForm>()
                .Property(e => e.FormId)
                .IsUnicode(false);

            modelBuilder.Entity<AllForm>()
                .Property(e => e.CreateAccount)
                .IsUnicode(false);

            modelBuilder.Entity<AllForm>()
                .Property(e => e.UpdateAccount)
                .IsUnicode(false);

            modelBuilder.Entity<AllForm>()
                .Property(e => e.ProcessAccount)
                .IsUnicode(false);

            modelBuilder.Entity<Bank>()
                .Property(e => e.BankCode)
                .IsUnicode(false);

            modelBuilder.Entity<Bank>()
                .Property(e => e.BranchCode)
                .IsUnicode(false);

            modelBuilder.Entity<HomeObject>()
                .Property(e => e.FormId)
                .IsUnicode(false);

            modelBuilder.Entity<HomeObject>()
                .Property(e => e.CaseId)
                .IsUnicode(false);

            modelBuilder.Entity<HomeObject>()
                .Property(e => e.taxfile_guid)
                .IsUnicode(false);

            modelBuilder.Entity<HomeObject>()
                .Property(e => e.roomamount)
                .IsUnicode(false);

            modelBuilder.Entity<HomeObject>()
                .Property(e => e.carpositionnumber)
                .IsUnicode(false);

            modelBuilder.Entity<Region>()
                .Property(e => e.CityCode)
                .IsUnicode(false);

            modelBuilder.Entity<Region>()
                .Property(e => e.DistrictCode)
                .IsUnicode(false);

            modelBuilder.Entity<Region>()
                .Property(e => e.RoadCode)
                .IsFixedLength();
        }
    }
}
