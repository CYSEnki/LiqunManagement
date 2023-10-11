namespace LiqunManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Form.Excerpt")]
    public partial class Excerpt
    {
        [Key]
        public int ExcerptNo { get; set; }

        [Required]
        [StringLength(10)]
        public string CityCode { get; set; }

        [Required]
        [StringLength(20)]
        public string CityName { get; set; }

        [Required]
        [StringLength(10)]
        public string DistrictCode { get; set; }

        [Required]
        [StringLength(20)]
        public string DistrictName { get; set; }

        [Required]
        [StringLength(10)]
        public string OfficeCode { get; set; }

        [Required]
        [StringLength(20)]
        public string OfficeName { get; set; }

        [Column("Excerpt")]
        [StringLength(20)]
        public string Excerpt1 { get; set; }

        [StringLength(20)]
        public string ExcerptShort { get; set; }

        [StringLength(10)]
        public string ExcerptCode { get; set; }
    }
}
