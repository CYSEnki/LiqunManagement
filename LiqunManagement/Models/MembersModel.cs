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

        public virtual DbSet<Department> Department { get; set; }
        public virtual DbSet<EmployeeData> EmployeeData { get; set; }
        public virtual DbSet<Members> Members { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Department>()
                .Property(e => e.DivCode)
                .IsUnicode(false);

            modelBuilder.Entity<Department>()
                .Property(e => e.ParentDivCode)
                .IsUnicode(false);

            modelBuilder.Entity<Department>()
                .Property(e => e.ManageAccount)
                .IsUnicode(false);

            modelBuilder.Entity<Department>()
                .Property(e => e.Role)
                .IsUnicode(false);

            modelBuilder.Entity<EmployeeData>()
                .Property(e => e.Account)
                .IsUnicode(false);

            modelBuilder.Entity<EmployeeData>()
                .Property(e => e.DivCode)
                .IsUnicode(false);

            modelBuilder.Entity<EmployeeData>()
                .Property(e => e.CreateAccount)
                .IsUnicode(false);

            modelBuilder.Entity<EmployeeData>()
                .Property(e => e.UpdateAccount)
                .IsUnicode(false);

            modelBuilder.Entity<EmployeeData>()
                .Property(e => e.AssistantAccount)
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

            modelBuilder.Entity<Members>()
                .Property(e => e.Role)
                .IsUnicode(false);
        }
    }
}
