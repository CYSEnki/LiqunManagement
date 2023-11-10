namespace LiqunManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Form.LandLord")]
    public partial class LandLord
    {
        [Key]
        public int LandlordNo { get; set; }

        [StringLength(10)]
        public string FormID { get; set; }

        [StringLength(100)]
        public string CaseID { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(20)]
        public string Principal { get; set; }

        public int? Gender { get; set; }

        public DateTime? Birthday { get; set; }

        [StringLength(20)]
        public string IDNumber { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(20)]
        public string BankNo { get; set; }

        [StringLength(20)]
        public string BrancheNo { get; set; }

        [StringLength(20)]
        public string BankAccount { get; set; }

        [StringLength(50)]
        public string Address { get; set; }

        [StringLength(50)]
        public string AddressDetail { get; set; }

        [StringLength(50)]
        public string ContactAddress { get; set; }

        [StringLength(50)]
        public string ContactAddressDetail { get; set; }

        [StringLength(50)]
        public string MemberArray { get; set; }

        [StringLength(200)]
        public string CoOwner1 { get; set; }

        [StringLength(200)]
        public string CoOwner2 { get; set; }

        [StringLength(200)]
        public string CoOwner3 { get; set; }

        [StringLength(200)]
        public string CoOwner4 { get; set; }

        [StringLength(200)]
        public string CoOwner5 { get; set; }

        [StringLength(200)]
        public string Agent { get; set; }

        public DateTime? CreateTime { get; set; }

        [StringLength(30)]
        public string CreateAccount { get; set; }

        public DateTime? UpdateTime { get; set; }

        [StringLength(30)]
        public string UpdateAccount { get; set; }

        [StringLength(300)]
        public string Memo { get; set; }
    }
}
