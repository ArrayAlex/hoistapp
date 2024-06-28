using hoistmt.Data;
using Microsoft.EntityFrameworkCore;

using MySqlConnector;

namespace hoistmt.Services;

public class TokenHandler
{
    private readonly MasterDbContext _context;

    public TokenHandler(MasterDbContext context)
    {
        _context = context;
    }

    public async Task<bool> VerifyToken(string token)
    {
        try
        {
            // Prepare the SQL command
            var queryString = "SELECT COUNT(*) FROM sessions WHERE Token = @Token AND ExpiresAt > NOW();";

            // Execute the command and retrieve the result
            await using var connection = new MySqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();
                
            await using var command = new MySqlCommand(queryString, connection);
            command.Parameters.AddWithValue("@Token", token);

            // Execute the command and retrieve the result
            var result = await command.ExecuteScalarAsync();

            // Convert the result to an integer
            var tokenCount = Convert.ToInt32(result);

            // If token count is greater than 0, token is valid
            return tokenCount > 0;
        }
        catch (Exception ex)
        {
            // Log or handle the exception
                
            return false; // Return false in case of any exception
        }
    }
}