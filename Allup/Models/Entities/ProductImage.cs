using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Allup.Models.Entities
{
    public class ProductImage : BaseEntity
    {
        public string Name { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public bool IsMain { get; set; }
        public int Order { get; set; }
    }
}
