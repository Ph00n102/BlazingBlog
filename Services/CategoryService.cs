using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.Xml;
using System.Text.RegularExpressions;
using BlazingBlog.Extensions;

namespace BlazingBlog.Services
{
    public class CategoryService
    {
         private readonly BlogContext _context;

        public CategoryService(BlogContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync() =>
            await _context.Categories
            .AsNoTracking()
            .ToListAsync();
         public async Task<MethodResult> SaveCategoryAsync(Category model)
        {
            try
            {
                if (model.Id > 0)
                {
                    // update category
                    _context.Categories.Update(model);
                }
                else
                {
                    // create category
                    model.Slug = model.Slug.Slugify();
                    await _context.Categories.AddAsync(model);
                }
                await _context.SaveChangesAsync();
                return MethodResult.Succes();
            }
            catch (Exception ex)
            {
                // log exception
                return MethodResult.Failure(ex.Message);
            }
        }
    }
}