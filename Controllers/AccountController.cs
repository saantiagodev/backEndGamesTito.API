using backEndGamesTito.API.Models;
using BackEndGamesTito.API.Models;
using BackEndGamesTito.API.Repositories;
using BackEndGamesTito.API.Services; // Namespace dos Serviços
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DbUsuario = BackEndGamesTito.API.Data.Models.Usuario;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace BackEndGamesTito.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UsuarioRepository _usuarioRepository;
        private readonly EmailService _emailService;
        private readonly TelegramService _telegramService; // 1. Novo Serviço Adicionado

        // 2. Construtor atualizado recebendo os TRÊS serviços
        public AccountController(
            UsuarioRepository usuarioRepository,
            EmailService emailService,
            TelegramService telegramService)
        {
            _usuarioRepository = usuarioRepository;
            _emailService = emailService;
            _telegramService = telegramService;
        }

        // --- DTOs ---
        public record ForgotPasswordDto(string Email);
        public record ResetPasswordDto(string Token, string NewPassword);

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestModel model)
        {
            try
            {
                DateTime agora = DateTime.Now;
                string dataString = agora.ToString();
                string ApiKey = "mangaPara_todos_ComLeite_kkk";

                string PassSHA256 = ComputeSha256Hash(model.PassWordHash);
                string EmailSHA256 = ComputeSha256Hash(model.Email);

                string PassCrip = PassSHA256 + EmailSHA256 + ApiKey;
                string HashCrip = EmailSHA256 + PassSHA256 + dataString + ApiKey;

                string PassBCrypt = BCrypt.Net.BCrypt.HashPassword(PassCrip);
                string HashBCrypt = BCrypt.Net.BCrypt.HashPassword(HashCrip);

                var novoUsuario = new DbUsuario
                {
                    NomeCompleto = model.NomeCompleto,
                    Email = model.Email,
                    PassWordHash = PassBCrypt,
                    HashPass = HashBCrypt,
                    DataAtualizacao = DateTime.Now,
                    StatusId = 2,
                    // Se você já adicionou a coluna no banco, pode receber o ChatId aqui no futuro
                    // TelegramChatId = model.TelegramChatId 
                };

                await _usuarioRepository.CreateUserAsync(novoUsuario);

                return Ok(new
                {
                    erro = false,
                    message = "Usuário cadastrado com sucesso!",
                    usuario = new { model.NomeCompleto, model.Email }
                });
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                return Conflict(new { erro = true, message = "Este email já está em uso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro: {ex.Message}" });
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel model)
        {
            var user = await _usuarioRepository.GetUserByEmailAsync(model.Email);

            if (user == null)
                return Unauthorized(new { erro = true, message = "Usuário ou senha inválidos." });

            // Validação da Senha (Mantenha sua lógica de Hash aqui)
            string ApiKey = "mangaPara_todos_ComLeite_kkk";
            string PassSHA256 = ComputeSha256Hash(model.PassWordHash);
            string EmailSHA256 = ComputeSha256Hash(model.Email);
            string PassCrip = PassSHA256 + EmailSHA256 + ApiKey;

            bool isPasswordValid = false;
            try
            {
                isPasswordValid = BCrypt.Net.BCrypt.Verify(PassCrip, user.PassWordHash);
            }
            catch (Exception) { isPasswordValid = false; }

            if (!isPasswordValid)
                return Unauthorized(new { erro = true, message = "Usuário ou senha inválidos." });

            // --- AQUI COMEÇA A MÁGICA DO TOKEN ---

            // 1. Gera o Token (O Crachá)
            var tokenHandler = new JwtSecurityTokenHandler();
            // USE A MESMA CHAVE DO PROGRAM.CS
            var key = Encoding.ASCII.GetBytes("UmaSenhaMuitoForteEMuitoSecretaParaOProjetoGamesTito123");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Email), // Guarda o email dentro do token
                    new Claim("id", user.UsuarioId.ToString()) // Guarda o ID
                }),
                Expires = DateTime.UtcNow.AddHours(8), // Token vale por 8 horas
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // 2. Retorna o Token para o usuário
            return Ok(new
            {
                erro = false,
                message = "Usuário logado!",
                token = tokenString, // <--- O TOKEN AGORA EXISTE!
                usuario = new { email = user.Email, nome = user.NomeCompleto }
            });
        }

        // --- RECUPERAÇÃO DE SENHA (EMAIL + TELEGRAM) ---

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            var user = await _usuarioRepository.GetUserByEmailAsync(model.Email);

            // Segurança: Se não achar, responde OK para não revelar quem é cliente
            if (user == null)
            {
                return Ok(new { message = "Se o e-mail existir, as instruções foram enviadas." });
            }

            // 1. Gera o Token e Validade
            string token = Guid.NewGuid().ToString();
            DateTime expiry = DateTime.Now.AddHours(1);

            // 2. Salva no Banco
            await _usuarioRepository.SaveResetTokenAsync(user.Email, token, expiry);

            // 3. Prepara o Link
            string link = $"https://localhost:7236/reset-password?token={token}";

            // --- ENVIO A: E-MAIL (Sempre tenta enviar) ---
            try
            {
                await _emailService.SendResetEmail(user.Email, token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar Email: {ex.Message}");
                // Não paramos o código aqui, tentamos o Telegram mesmo assim
            }

            // --- ENVIO B: TELEGRAM (Só se o usuário tiver ID cadastrado) ---
            // IMPORTANTE: Certifique-se que seu Usuario.cs tem a propriedade TelegramChatId
            if (!string.IsNullOrEmpty(user.TelegramChatId))
            {
                await _telegramService.EnviarLink(user.TelegramChatId, link);
            }

            return Ok(new { message = "Se o e-mail existir, as instruções foram enviadas." });
        }

        [HttpPost("reset-password")] /* ROTA PARA RECEBER O NOVO TOKEN E SENHA */
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            var user = await _usuarioRepository.GetUserByResetTokenAsync(model.Token);

            if (user == null || user.ResetTokenExpiry < DateTime.Now)
            {
                return BadRequest(new { erro = true, message = "Token inválido ou expirado." });
            }

            try
            {
                string ApiKey = "mangaPara_todos_ComLeite_kkk";
                string PassSHA256 = ComputeSha256Hash(model.NewPassword);
                string EmailSHA256 = ComputeSha256Hash(user.Email);
                string PassCrip = PassSHA256 + EmailSHA256 + ApiKey;
                string novoHashFinal = BCrypt.Net.BCrypt.HashPassword(PassCrip);

                await _usuarioRepository.UpdatePasswordAsync(user.UsuarioId, novoHashFinal);

                return Ok(new { message = "Senha redefinida com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao redefinir senha.", detalhe = ex.Message });
            }
        }

        [HttpGet("perfil")] /* IDENTIFICAR PERFIL DO USUARIO */
        [Authorize]
        public async Task<IActionResult> GetPerfil()
        {
            try
            {
                var emailLogado = User.Identity?.Name;

                if (string.IsNullOrEmpty(emailLogado))
                    return Unauthorized(new { message = "Token inválido." });

                var user = await _usuarioRepository.GetUserByEmailAsync(emailLogado);

                if (user == null)
                    return NotFound(new { message = "Usuário não encontrado." });

                return Ok(new
                {
                    id = user.UsuarioId,
                    nome = user.NomeCompleto,
                    email = user.Email,
                    telegramConectado = !string.IsNullOrEmpty(user.TelegramChatId),
                    mensagem = $"Olá, {user.NomeCompleto}! Bem-vindo à sua área logada."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar perfil", erro = ex.Message });
            }
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }


}