using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Stories.Models
{
    [Table("Comment", Schema = "public")]
    public class Comment
    {
        [Column("id")]
        public Guid Id { get; set; }
    }
}
