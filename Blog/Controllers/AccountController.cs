using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.Services;
using Blog.ViewModels;
using Blog.ViewModels.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureIdentity.Password;
using System.Text.RegularExpressions;

namespace Blog.Controllers
{
    [ApiController]
    public class AccountController : ControllerBase
    {
        [HttpPost("v1/accounts/")]
        public async Task<IActionResult> Post(
           [FromBody] RegisterViewModel model,
           [FromServices] EmailService emailService,
           [FromServices] BlogDataContext context)
        {
            if (!ModelState.IsValid) return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));
            
            try
            {
                var user = new User
                {
                    Name = model.Name,
                    Email = model.Email,
                    Slug = model.Email.Replace("@", "-").Replace(".", "-"),
                };
                var password = PasswordGenerator.Generate(25);
                user.PasswordHash = PasswordHasher.Hash(password);

                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();
                emailService.Send(user.Name, user.Email, "Bem vindo ao nosso site", $"Sua senha é <strong>{password}<strong>");

                return Ok(new ResultViewModel<dynamic>(new
                {
                    user = user.Email, password
                }));
            }
            catch
            {
                return BadRequest(new ResultViewModel<string>("Erro ao criar usuário"));
            }
        }

        [HttpPost("v1/accounts/login")]
        public async Task<IActionResult> Login([FromServices] BlogDataContext context, [FromBody] LoginViewModel model, [FromServices]TokenService tokenService)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));
            var user = await context
                .Users
                .AsNoTracking()
                .Include(x => x.Roles)
                .FirstOrDefaultAsync(x => x.Email == model.Email);

            if (user == null) return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválido"));
            if (PasswordHasher.Verify(user.PasswordHash, model.Password))
                return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválido"));
            try
            {
                var token = tokenService.GenerateToken(user);
                return Ok(new ResultViewModel<string>(token, null));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<string>("Falha interna no servidor"));
            }
        }

        [Authorize]
        [HttpPost("v1/accounts/upload-image")]
        public async Task<IActionResult> UploadImage(
            [FromBody] UploadImageViewModel model,
            [FromServices] BlogDataContext context)
        {
            var fileName = $"{Guid.NewGuid().ToString()}.jpg";
            var data = new Regex(@"^data:image\/[a-z]+;base64,").Replace(model.Base64Image, "");
            var bytes = Convert.FromBase64String(data);
            try
            {
                await System.IO.File.WriteAllBytesAsync($"wwwroot/images/{fileName}", bytes);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>("Falha interna no servidor"));
            }

            var user = await context
                .Users
                .FirstOrDefaultAsync(x => x.Email == User.Identity.Name);

            if (user == null) return NotFound(new ResultViewModel<User>("Usuário não encontrado"));

            user.Image = $"https://localhost:0000/images/{fileName}";

            try
            {
                context.Users.Update(user);
                await context.SaveChangesAsync();
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<string>("Falha interna no servidor"));
            }

            return Ok(new ResultViewModel<string>("Imagem alterado com sucesso!", null));

        }
    }
}
