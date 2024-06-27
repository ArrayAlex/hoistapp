using hoistmt.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using hoistmt.Models;

namespace hoistmt.Services
{
    public class TenantService
    {
        private readonly MasterDbContext _context;

        public TenantService(MasterDbContext context)
        {
            _context = context;
        }

        public async Task<Tenant> CreateTenant(newUser newUser)
        {

            Tenant tenant = new Tenant {
                Name = newUser.Name,
                Username = newUser.Username,
                Password = newUser.Password,
                DatabaseName = newUser.DatabaseName
            };

            
            
            var existingTenant = await _context.Tenants.FirstOrDefaultAsync(t => t.DatabaseName == tenant.DatabaseName);
            if (existingTenant != null)
            {
                // DatabaseName is already taken, return appropriate response
                throw new ConflictException("DatabaseName is already taken.");
            }
            // Add the new tenant to the Tenants table
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            // Retrieve tables from the template schema (client_bbt_0001)
            var tablesInTemplateSchema = await GetTablesInTemplateSchema();

            // Check if the template schema exists
            if (!tablesInTemplateSchema.Any())
            {
                throw new Exception("No tables found in the template schema.");
            }

            // Create a new schema with the name specified in DatabaseName property
            var newSchemaName = tenant.DatabaseName;
            var createSchemaSql = $"CREATE SCHEMA `{newSchemaName}`";
            await _context.Database.ExecuteSqlRawAsync(createSchemaSql);
            var templateSchemaName = "templateSchema";
            // Copy structure and data from the template schema to the new schema
            foreach (var table in tablesInTemplateSchema)
            {
                var copyTableSql = $"CREATE TABLE `{newSchemaName}`.`{table}` LIKE `{templateSchemaName}`.`{table}`;" +
                                   $"INSERT INTO `{newSchemaName}`.`{table}` SELECT * FROM `{templateSchemaName}`.`{table}`";
                await _context.Database.ExecuteSqlRawAsync(copyTableSql);
            }
            
            User user = new User
            {
                Name = newUser.Name,
                Password = newUser.Password,
                Username = newUser.Username,
                email = newUser.email,
                phone = newUser.phone,
                roleID = 1,
                Active = 1,
                roleName = "Admin"
            };
            // Insert tenant data into the accounts table for the new schema
            await InsertTenantIntoAccounts(user, newSchemaName);

            return tenant;
        }

        private async Task InsertTenantIntoAccounts(User user, string schemaName)
        {
            // Determine the value for the phone field
            string phoneValue = string.IsNullOrEmpty(user.phone) ? "NULL" : $"'{user.phone}'";

            // Construct the SQL insert statement
            var insertSql = $"INSERT INTO `{schemaName}`.accounts (Name, Password, Username, email, Active, roleID, roleName, phone) " +
                            $"VALUES ('{user.Name}', '{user.Password}', '{user.Username}', '{user.email}', {user.Active}, {user.roleID}, '{user.roleName}', {phoneValue})";

            // Log the SQL statement before executing

            // Execute the SQL command
            await _context.Database.ExecuteSqlRawAsync(insertSql);
        }

        private async Task<List<string>> GetTablesInTemplateSchema()
        {
            // Assuming the template schema name is 'client_bbt_0001'
            var templateSchemaName = "templateSchema";

            // Retrieve table names from INFORMATION_SCHEMA.TABLES
            var tables = new List<string>();
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = $"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{templateSchemaName}'";
                _context.Database.OpenConnection();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        tables.Add(reader.GetString(0));
                    }
                }
            }

            return tables;
        }
    }
}
