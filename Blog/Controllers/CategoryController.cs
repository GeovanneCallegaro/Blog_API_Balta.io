using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.ViewModels;
using Blog.ViewModels.Categories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Blog.Controllers
{
    [ApiController]
    public class CategoryController : ControllerBase
    {
        [HttpGet("v1/categories")]
        public async Task<IActionResult> GetAsync(
            [FromServices] IMemoryCache cache,
            [FromServices] BlogDataContext context)
        {
            try
            {
                var categories = cache.GetOrCreate("CategoriesCache", entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                    return GetCategories(context);
                });
                return Ok(new ResultViewModel<List<Category>>(categories));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<List<Category>>("05X04 - Falha interna no servidor"));
            }   
        }

        private List<Category> GetCategories(BlogDataContext context)
        {
            return context.Categories.ToList();
        }


        [HttpGet("v1/categories/{id:int}")]
        public async Task<IActionResult> GetByIdAsync(
            [FromServices] BlogDataContext context,
            [FromRoute] int id)
        {
            try
            {
                var category = await
                context.Categories
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null) return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado!"));

                return Ok(category);
            } catch
            {
                return StatusCode(500, new ResultViewModel<Category>("Falha interna no servidor"));
            }

        }

        [HttpPost("v1/categories")]
        public async Task<IActionResult> PostAsync(
                [FromServices] BlogDataContext context,
                [FromBody] EditorCategoryViewModel model)
        {
            if(!ModelState.IsValid) 
                return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));

            try
            {
                var category = new Category
                {
                    Name = model.Name,
                    Slug = model.Slug,
                };
                await context.Categories.AddAsync(category);
                await context.SaveChangesAsync();

                return Created($"v1/categories/{category.Id}", new ResultViewModel<Category>(category));
            } 
            catch
            {
                return BadRequest(new ResultViewModel<Category>("Não foi possível incluir a categoria"));
            }
        }

        [HttpPut("v1/categories/{id:int}")]
        public async Task<IActionResult> PutAsync(
        [FromServices] BlogDataContext context,
        [FromBody] EditorCategoryViewModel model,
        [FromRoute] int id)
        {

            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));
            try
            {
                var category = await context.Categories
                    .FirstOrDefaultAsync(c => c.Id == id);
                if (category == null) return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado!"));

                category.Name = model.Name;
                category.Slug = model.Slug;

                context.Categories.Update(category);
                await context.SaveChangesAsync();

                return Ok(new ResultViewModel<Category>(category));
            }
            catch
            {
                return BadRequest(new ResultViewModel<Category>("Não foi possível atualizar a categoria"));
            }
        }

        [HttpDelete("v1/categories/{id:int}")]
        public async Task<IActionResult> DeleteAsync(
        [FromServices] BlogDataContext context,
        [FromRoute] int id)
        {
            try
            {
                var category = await context.Categories
                    .FirstOrDefaultAsync(c => c.Id == id);
                if (category == null) return NotFound(new ResultViewModel<Category>("Não foi possível encontrar o conteúdo!"));

                context.Categories.Remove(category);
                await context.SaveChangesAsync();

                return Ok(new ResultViewModel<Category>(category));
            }
            catch
            {
                return BadRequest(new ResultViewModel<Category>("Não foi possível deletar a categoria"));
            }

        }
    }
}
