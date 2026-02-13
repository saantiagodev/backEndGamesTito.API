using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BackEndGamesTito.API.Services
{
    public class TelegramService
    {
        // ⚠️ COLOQUE AQUI O TOKEN QUE O @BotFather TE DEU
        private readonly string _botToken = "8391842730:AAFQ_pyVhw_w3rRHQrW9nLnrB2lZZe_L6P4";
        private readonly HttpClient _httpClient;

        public TelegramService()
        {
            _httpClient = new HttpClient();
        }

        public async Task EnviarLink(string chatId, string link)
        {
            if (string.IsNullOrEmpty(chatId)) return; // Se não tiver ID, não faz nada

            string mensagem = $"🔐 <b>Games Tito - Recuperação</b>\n\nVocê pediu para resetar a senha.\nClique no link abaixo:\n\n{link}";

            // O Telegram exige que a mensagem esteja codificada para URL (espaços viram %20, etc)
            string mensagemCodificada = Uri.EscapeDataString(mensagem);

            // Monta a URL mágica
            string url = $"https://api.telegram.org/bot{_botToken}/sendMessage?chat_id={chatId}&text={mensagemCodificada}&parse_mode=HTML";

            try
            {
                await _httpClient.GetAsync(url);
                Console.WriteLine($"[TELEGRAM] Mensagem enviada para {chatId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO TELEGRAM] {ex.Message}");
                // Não damos 'throw' aqui para não travar o envio do e-mail se o Telegram falhar
            }
        }
    }
}
