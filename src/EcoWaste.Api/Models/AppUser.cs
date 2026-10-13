using System.ComponentModel.DataAnnotations;

namespace EcoWaste.Api.Models;

/// <summary>
/// Usuário do sistema usado apenas para autenticação/autorização dos endpoints críticos.
/// (Não é o foco do projeto — existe só para proteger as rotas sensíveis.)
/// </summary>
public class AppUser
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Ex.: "Admin", "Operator".</summary>
    [Required, MaxLength(40)]
    public string Role { get; set; } = "Operator";
}
