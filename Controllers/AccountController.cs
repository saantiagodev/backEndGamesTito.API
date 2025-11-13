// --- Controllers/AccountController.cs

using BackEndGamesTito.API.Models;
using BCrypt.Net; // Biblioteca BCrypt para hashing de senhas
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
// --- ADICIONAR ELEMENTOS PARA CRIPTOGRAFIA --- //

using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
// Adicionar um repositório para gerenciar a lógica de dados

using System.Threading.Tasks;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

// Usar o banco de dados de dados com o DbUsuario e os atributos de classe usuario
using DbUsuario = BackEndGamesTito.API.Data.Models.Usuario;
using BackEndGamesTito.Repositories;


namespace BackEndGamesTito.API.Controllers
{
    // Criando as rotas para o controller de conta
    [ApiController]
    [Route("api/[controller]")] // controle de rotas é o meu proprio endpoint
    /*public class AccountController : ControllerBase
    {

    }*/

    public class AccountController
    {
        /*try
        {
        // Converte data para string
        string dataString = agora.ToString();
        // Palavra passe
        string ApiKey = "mangaPara_todos_ComLeite_kkk";

        // Cria a senha e email aplicando SHA256
        string PassSHA256 = ComputeSha256Hash(model.PasswordHash);
        string EmailSHA256 = ComputeSha256Hash(model.Email);

        // Criando a string para a criptografia da senha e hash(para recuperar senha)
        string PassCrip = PassSHA256 + EmailSHA256 + ApiKey;

        string HashCrip = EmailSHA256 + PassSHA256 + dataString + ApiKey;

        // Aplicando o BCrypt
        string PassBCrypt = BCrypt.Net.BCrypt.HashPassword(PassCrip);
        string HashBCrypt = BCrypt.Net.BCrypt.HashPassword(HashCrip);

        // Criando o 'array' com todos os dados do usuario para depois ser gravado
        var novoUsuario = new DbUsuario {
            Email = model.Email,
            PasswordHash = PassBCrypt,
            HashRecovery = HashBCrypt,
            DataCriacao = agora,
            DataExpiracaoHash = dataExpiracao
        };

        await _usuarioRepository.CreateUserAsync(novoUsuario);
        }*/

    }
}

/*// CRIA UMA INSTRANCIA DE SH256 || METODO DO SH256 
private string ComputeSha256Hash(string rawData)
{
    using (SHA256 sha256Hash = SHA256.Create())
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            // Computa o hash do dado de entrada 'string'
            // Retorna o resultado como um array  de bytes
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            // Converte o array de bytes para uma string hexadecimal
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}*/
