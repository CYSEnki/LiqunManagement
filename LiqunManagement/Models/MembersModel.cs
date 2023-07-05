using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace LiqunManagement.Models
{
    public partial class MembersModel : DbContext
    {
        public MembersModel()
            : base("name=MembersModel")
        {
        }

        public virtual DbSet<Members> Members { get; set; }
        public virtual DbSet<Region> Region { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Members>()
                .Property(e => e.Account)
                .IsUnicode(false);

            modelBuilder.Entity<Members>()
                .Property(e => e.Password)
                .IsUnicode(false);

            modelBuilder.Entity<Members>()
                .Property(e => e.Email)
                .IsUnicode(false);

            modelBuilder.Entity<Members>()
                .Property(e => e.AuthCode)
                .IsFixedLength();

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
