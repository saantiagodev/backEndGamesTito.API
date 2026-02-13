using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System;

namespace BackEndGamesTito.API.Services
{
    public class EmailService
    {
        public async Task SendResetEmail(string emailDestino, string token)
        {
            // --- CONFIGURAÇÕES DO GMAIL (Preencha aqui) ---
            string meuEmail = "sasntdy@gmail.com";   // <--- SEU GMAIL AQUI
            string minhaSenhaApp = "gaqf lawk fkpn vqnb";     // <--- A SENHA DE 16 LETRAS QUE O GOOGLE GEROU
            

            // Monta o link (apontando para o seu Front ou API)
            string link = $"https://localhost:7236/reset-password?token={token}";

            var mensagem = new MailMessage();
            mensagem.From = new MailAddress(meuEmail, "Suporte Games Tito");
            mensagem.To.Add(new MailAddress(emailDestino));
            mensagem.Subject = "Recuperação de Senha - Games Tito";
            mensagem.Body = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                    <h2 style='color: #2c3e50;'>Olá!</h2>
                    <p>Recebemos um pedido para redefinir sua senha.</p>
                    <p>Se foi você, clique no botão abaixo:</p>
                    <a href='{link}' style='background-color: #3498db; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;'>REDEFINIR MINHA SENHA</a>
                    <p style='margin-top: 20px; font-size: 12px; color: #777;'>Este link expira em 1 hora.</p>
                    <hr>
                    <p style='font-size: 11px; color: #999;'>Se não foi você, ignore este e-mail.</p>
                </div>";
            mensagem.IsBodyHtml = true;

            using (var smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(meuEmail, minhaSenhaApp);

                try
                {
                    await smtp.SendMailAsync(mensagem);
                    Console.WriteLine($"[SUCESSO] E-mail REAL enviado para {emailDestino}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERRO GMAIL] {ex.Message}");
                    throw; // Repassa o erro para sabermos se falhou
                }
            }
        }
    }
}
