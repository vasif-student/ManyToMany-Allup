using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Allup.Areas.Admin.ViewModels
{
    public class CategoryUpdateViewModel
    {
        public string Name { get; set; }
        public string imageName { get; set; }
        public IFormFile File { get; set; }
        public bool IsMain { get; set; }
        public int ParentId { get; set; }
    }
}
