namespace BackEndGamesTito.API.Data.Models
{
    public class Usuario
    {
        public int UsuarioId { get; set; }
        public string NomeCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PassWordHash { get; set; } = string.Empty;
        public string HashPass {  get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        public int StatusId { get; set; }

        // void só posso  utilizar dentro de funções ou métodos
        // nulo é a inexistencia, é um valor que não existe
        // // empty é uma string vazia

    }
}
