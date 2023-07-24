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
        public string FormId { get; set; }

        [StringLength(10)]
        public string Lordtype { get; set; }

        [StringLength(20)]
        public string Name { get; set; }

        public DateTime? Birthday { get; set; }

        [StringLength(20)]
        public string IDNumber { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [StringLength(10)]
        public string RoadCode { get; set; }

        [StringLength(50)]
        public string elseaddress { get; set; }

        [StringLength(200)]
        public string fulladdress { get; set; }

        [StringLength(10)]
        public string RoadCodeContact { get; set; }

        [StringLength(50)]
        public string elseaddressContact { get; set; }

        [StringLength(200)]
        public string fulladdressContact { get; set; }

        public DateTime? CreateTime { get; set; }

        [StringLength(50)]
        public string CreateUser { get; set; }

        public DateTime? UpdateTime { get; set; }

        [StringLength(50)]
        public string UpdateUser { get; set; }
    }
}
