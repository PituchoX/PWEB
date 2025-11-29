using Microsoft.AspNetCore.Identity;

namespace RESTfulAPIPWeb.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string Nome { get; set; } = "";
        public string NIF { get; set; } = "";
        public string Estado { get; set; } = "Pendente";
    }
}
