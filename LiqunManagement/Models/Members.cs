namespace LiqunManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("LOG.Members")]
    public partial class Members
    {
        [Key]
        [StringLength(30)]
        public string Account { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Email { get; set; }

        [StringLength(10)]
        public string AuthCode { get; set; }

        public bool IsAdmin { get; set; }

        public bool Status { get; set; }

        [StringLength(10)]
        public string Role { get; set; }
    }
}
