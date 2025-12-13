using Microsoft.AspNetCore.Identity;

namespace RESTfulAPIPWeb.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string NomeCompleto { get; set; } = string.Empty;

        // Estado: Ativo / Inativo
        public string Estado { get; set; } = "Ativo";

        // Role principal do utilizador (Administrador, Funcionário, Cliente, Fornecedor)
        public string Perfil { get; set; } = "Cliente";
    }
}
