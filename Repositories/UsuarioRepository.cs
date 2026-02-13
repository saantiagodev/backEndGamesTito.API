using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using BackEndGamesTito.API.Models;
using BackEndGamesTito.API.Data.Models;

namespace BackEndGamesTito.API.Repositories
{
    public class UsuarioRepository
    {
        private readonly string _connectionString = string.Empty;

        public UsuarioRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("String de conexão 'DefaultConnection' não encontrada!");
        }

        public async Task CreateUserAsync(Usuario user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var commandText = @"
                    INSERT INTO dbo.Usuario 
                    (NomeCompleto, Email, PassWordHash, HashPass, DataAtualizacao, StatusId)
                    VALUES 
                    (@NomeCompleto, @Email, @PassWordHash, @HashPass, @DataAtualizacao, @StatusId)
                ;";

                using (var command = new SqlCommand(commandText, connection))
                {
                    command.Parameters.AddWithValue("@NomeCompleto", user.NomeCompleto);
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@PassWordHash", user.PassWordHash);
                    command.Parameters.AddWithValue("@HashPass", user.HashPass);
                    // Esta Linha da 'DataAtualizacao entrada como objeto podendo ser um valor 'nulo'
                    command.Parameters.AddWithValue("@DataAtualizacao", (object?)user.DataAtualizacao ?? DBNull.Value);
                    command.Parameters.AddWithValue("@StatusId", user.StatusId);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<Usuario?> GetUserByEmailAsync(string email)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var commandText = @"
                    SELECT TOP 1 * FROM dbo.Usuario 
                    WHERE Email = @Email
                ";

                using (var command = new SqlCommand(commandText, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            //Mapeia os dados do banco para o objeto 'Usuario'
                            return new Usuario
                            {
                               UsuarioId = reader.GetInt32(reader.GetOrdinal("UsuarioId")),
                               NomeCompleto = reader.GetString(reader.GetOrdinal("NomeCompleto")),
                               Email = reader.GetString(reader.GetOrdinal("Email")),
                               PassWordHash = reader.GetString(reader.GetOrdinal("PassWordHash")),
                               HashPass = reader.GetString(reader.GetOrdinal("HashPass")),
                               DataCriacao = reader.GetDateTime(reader.GetOrdinal("DataCriacao")),
                               DataAtualizacao = reader.IsDBNull(reader.GetOrdinal("DataAtualizacao")) 
                                    ? null : reader.GetDateTime(reader.GetOrdinal("DataAtualizacao")),
                               StatusId = reader.GetInt32(reader.GetOrdinal("StatusId"))

                            };
                        }
                    }
                }
                return null; // Retorna nulo se o usuário não for encontrado
            }
        }




        // ... imports existentes

        // Método para salvar o token de recuperação
        public async Task SaveResetTokenAsync(string email, string token, DateTime expiry)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                // Atualiza apenas se o usuário existir
                var commandText = @"
            UPDATE dbo.Usuario 
            SET ResetToken = @ResetToken, 
                ResetTokenExpiry = @ResetTokenExpiry 
            WHERE Email = @Email";

                using (var command = new SqlCommand(commandText, connection))
                {
                    command.Parameters.AddWithValue("@ResetToken", token);
                    command.Parameters.AddWithValue("@ResetTokenExpiry", expiry);
                    command.Parameters.AddWithValue("@Email", email);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        // Método para buscar usuário pelo Token (para validar)
        public async Task<Usuario?> GetUserByResetTokenAsync(string token)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var commandText = "SELECT TOP 1 * FROM dbo.Usuario WHERE ResetToken = @Token";

                using (var command = new SqlCommand(commandText, connection))
                {
                    command.Parameters.AddWithValue("@Token", token);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            // Reutilize a lógica de mapeamento existente ou crie um método privado 'MapUsuario'
                            return new Usuario
                            {
                                UsuarioId = reader.GetInt32(reader.GetOrdinal("UsuarioId")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                PassWordHash = reader.GetString(reader.GetOrdinal("PassWordHash")),
                                // Mapear os novos campos
                                ResetToken = reader.IsDBNull(reader.GetOrdinal("ResetToken")) ? null : reader.GetString(reader.GetOrdinal("ResetToken")),
                                ResetTokenExpiry = reader.IsDBNull(reader.GetOrdinal("ResetTokenExpiry")) ? null : reader.GetDateTime(reader.GetOrdinal("ResetTokenExpiry"))
                            };
                        }
                    }
                }
            }
            return null;
        }

        // Método para Atualizar a Senha e limpar o Token
        public async Task UpdatePasswordAsync(int usuarioId, string newPasswordHash)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var commandText = @"
            UPDATE dbo.Usuario 
            SET PassWordHash = @PassWordHash, 
                ResetToken = NULL, 
                ResetTokenExpiry = NULL 
            WHERE UsuarioId = @UsuarioId";

                using (var command = new SqlCommand(commandText, connection))
                {
                    command.Parameters.AddWithValue("@PassWordHash", newPasswordHash);
                    command.Parameters.AddWithValue("@UsuarioId", usuarioId);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
