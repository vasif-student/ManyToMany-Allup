using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Allup.Models.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; }
        public int Rating { get; set; }
        public double Price { get; set; }
        public bool IsDiscounted { get; set; }
        public double? DiscountedPrice { get; set; }
        public string Tax { get; set; }
        public string ProductCode { get; set; }
        public string Description { get; set; }
        public string Brand { get; set; }
        public ICollection<ProductImage> Images { get; set; }
        public ICollection<ProductCategory> ProductCategories { get; set; }
    }
}
