using Allup.Areas.Admin.ViewModels;
using Allup.Data;
using Allup.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Allup.Areas.Admin.Constants;
using Allup.Areas.Admin.Utilis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace Allup.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.Where(c => !c.IsDeleted).ToListAsync();
            return View(categories);
        }

        //***** Detail *****//
        public async Task<IActionResult> Detail(int id)
        {
            var category = await _context.Categories.Include(c => c.Children).FirstOrDefaultAsync(c => c.Id == id);
            if(category == null)
            {
                return NotFound();
            }

            return View(category);
        }


        //***** Create *****//
        public async Task<IActionResult> Create()
        {
            var parents = await _context.Categories.Where(c => c.IsMain && !c.IsDeleted).ToListAsync();
            ViewBag.Parents = parents;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryCreateViewModel model)
        {
            var parents = await _context.Categories.Where(c => c.IsMain && !c.IsDeleted).ToListAsync();
            ViewBag.Parents = parents;

            if (!ModelState.IsValid)
            {
                return View();
            }

            if(model.IsMain)
            {
                if(model.File == null)
                {
                    ModelState.AddModelError("File", "Select an image");
                    return View();
                }
                if(!model.File.IsSupported())
                {
                    ModelState.AddModelError("File", "File is unsupported");
                    return View();
                }
                if(model.File.IsGreaterThanGivenSize(1024))
                {
                    ModelState.AddModelError(nameof(model.File), "File size cannot be greater than 1 mb");
                    return View();
                }

                var imageName = FileUtil.CreatedFile(FileConstants.ImagePath, model.File);

                Category category = new Category
                {
                    Name = model.Name,
                    Image = imageName,
                    IsMain = model.IsMain
                };

                await _context.Categories.AddAsync(category);
            }
            else
            {
                var parent = await _context.Categories.FirstOrDefaultAsync(c => c.IsMain && !c.IsDeleted && c.Id == model.ParentId);
                if(parent == null)
                {
                    ModelState.AddModelError("ParentId", "Choose valid category");
                    return View();
                }

                Category category = new Category
                {
                    Name = model.Name,
                    Image = "sehv",
                    IsMain = model.IsMain,
                    Parent = parent
                };

                await _context.Categories.AddAsync(category);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        //***** Delete *****//
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.Include(c => c.Parent)
                .Include(c => c.Children).FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.Include(c => c.Children).FirstOrDefaultAsync(c => !c.IsDeleted && c.Id == id);
            if (category == null) return NotFound();

            if(category.IsMain)
            {
                category.IsDeleted = true;
                foreach(var child in category.Children)
                {
                    child.IsDeleted = true;
                }
            }
            else
            {
                category.IsDeleted = true;
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        //***** Update *****//
        public async Task<IActionResult> Update(int id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            var parents = await _context.Categories.Where(c => c.IsMain && !c.IsDeleted).ToListAsync();
            ViewBag.Parents = parents;
            var categoryVM = new CategoryUpdateViewModel
            {
                Name = category.Name,
                IsMain = category.IsMain,
                imageName = category.Image
            };

            return View(categoryVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, CategoryUpdateViewModel model)
        {

            Category contextCategory = await _context.Categories.Include(c => c.Parent)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            var parents = await _context.Categories.Where(c => c.IsMain && !c.IsDeleted).ToListAsync();
            ViewBag.Parents = parents;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool isExist = await _context.Categories.AnyAsync(c => c.Id == id);

            if(!isExist)
            {
                return NotFound();
            }

            if (model.IsMain)
            {
                if(model.File == null)
                {
                    ModelState.AddModelError(nameof(CategoryUpdateViewModel.File), "Upload a file please");
                    return View(model);
                }
                FileUtil.DeleteFile(Path.Combine(FileConstants.ImagePath, contextCategory.Image));
                contextCategory.Name = model.Name;
                contextCategory.IsMain = model.IsMain;
                contextCategory.Image = FileUtil.CreatedFile(FileConstants.ImagePath, model.File);
            }
            else
            {
                contextCategory.Name = model.Name;
                contextCategory.Parent = await _context.Categories.FindAsync(model.ParentId);
                contextCategory.IsMain = model.IsMain;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }

    }
}
