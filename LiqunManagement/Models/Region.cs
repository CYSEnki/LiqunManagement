namespace LiqunManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Form.Region")]
    public partial class Region
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RegionNo { get; set; }

        [StringLength(20)]
        public string City { get; set; }

        [StringLength(10)]
        public string CityCode { get; set; }

        [StringLength(20)]
        public string District { get; set; }

        [StringLength(10)]
        public string DistrictCode { get; set; }

        [StringLength(20)]
        public string Road { get; set; }

        [StringLength(10)]
        public string RoadCode { get; set; }
    }
}
