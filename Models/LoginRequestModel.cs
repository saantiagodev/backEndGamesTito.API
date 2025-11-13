// Um arquivo de requisição
// Criar algo que seja sutil para quando eu faça a requisição a requisição venha

// como requisição meu cliente tem que fornecer o email e a senha para minha api preparar 

using System.ComponentModel.DataAnnotations; // Objeto nativo do C# para validação de dados por isso não precisamos colocar parentses na chamada do método ComponentModel.DataAnnotations

namespace BackEndGamesTito.API.Models
{
    public class LoginRequestModel
    {
        [Required(ErrorMessage = "O campo email é obrigatório.")] // O required faz o campo ter que ser obrigatório e o erroemessage define uma mensagem para caso o dado não seja fornecido
        [EmailAddress(ErrorMessage = "O Email informado não pe válido.")] // Verifica se o Email é válido e define uma mensagem de erro caso não seja válido
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "O campo senha é obrigatório.")]
        public string PasswordHash { get; set; } = string.Empty; // Por questões de segurança preiso pedir o email e a senha do usuário para a requisição
    }
}

// Toda entrega de API depende de uma requisição 