namespace LiqunManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Form.FileLog")]
    public partial class FileLog
    {
        [Key]
        public int FileNo { get; set; }

        [Required]
        [StringLength(10)]
        public string FormID { get; set; }

        [Required]
        [StringLength(20)]
        public string Category { get; set; }

        [Required]
        [StringLength(300)]
        public string FileNames { get; set; }

        [Required]
        [StringLength(1000)]
        public string FileAlias { get; set; }

        [Required]
        [StringLength(10)]
        public string FileExtension { get; set; }

        public DateTime CreateTime { get; set; }

        [Required]
        [StringLength(30)]
        public string CreateAccount { get; set; }

        public DateTime UpdateTime { get; set; }

        [Required]
        [StringLength(30)]
        public string UpdateAccount { get; set; }

        [StringLength(10)]
        public string FilePath { get; set; }
    }
}
