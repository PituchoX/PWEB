using Microsoft.AspNetCore.Identity;

namespace GestaoLoja.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string? NomeCompleto { get; set; }
    }
}
