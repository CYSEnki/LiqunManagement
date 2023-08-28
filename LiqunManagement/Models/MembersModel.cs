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

        public virtual DbSet<EmployeeData> EmployeeData { get; set; }
        public virtual DbSet<Members> Members { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmployeeData>()
                .Property(e => e.Account)
                .IsUnicode(false);

            modelBuilder.Entity<EmployeeData>()
                .Property(e => e.CreateAccount)
                .IsUnicode(false);

            modelBuilder.Entity<EmployeeData>()
                .Property(e => e.UpdateAccount)
                .IsUnicode(false);

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
        }
    }
}
