using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Stories.Models
{
    [Table("Author", Schema = "public")]
    public class Author
    {
        [Column("id")]
        public string Id { get; set; }
    }
}
