using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels.Accounts
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Nome obrigatório")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Email obrigatório")]
        [EmailAddress(ErrorMessage = "O email é inválido")]
        public string Email { get; set; }
    }
}
