using hoistmt.Data;
using hoistmt.Models;
using hoistmt.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using hoistmt.Exceptions;
using hoistmt.Interfaces;
using hoistmt.Models.MasterDbModels;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace hoistmt.Services
{
    public class TenantService
    {
        private readonly MasterDbContext _context;
        private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;
        private readonly EmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantService(MasterDbContext context, ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver, EmailService emailService,IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _tenantDbContextResolver = tenantDbContextResolver;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
            
        }

        public async Task<DbTenant> CreateTenant(newUser newUser)
        {
            if (string.IsNullOrEmpty(newUser.DatabaseName) || string.IsNullOrEmpty(newUser.Name) || string.IsNullOrEmpty(newUser.Email) || string.IsNullOrEmpty(newUser.Password) || string.IsNullOrEmpty(newUser.Username))
            {
                throw new ArgumentException("All fields are required.");
            }

            DbTenant dbTenant = new DbTenant
            {
                Name = newUser.Name,
                Username = newUser.Username,
                Password = newUser.Password,
                DatabaseName = newUser.DatabaseName,
            };

            var existingTenant = await _context.tenants.FirstOrDefaultAsync(t => t.DatabaseName == dbTenant.DatabaseName);
            if (existingTenant != null)
            {
                throw new InvalidOperationException("DatabaseName is already taken.");
            }

            _context.tenants.Add(dbTenant);
            await _context.SaveChangesAsync();

            var tablesInTemplateSchema = await GetTablesInTemplateSchema();

            if (!tablesInTemplateSchema.Any())
            {
                throw new Exception("No tables found in the template schema.");
            }

            var newSchemaName = dbTenant.DatabaseName;
            var templateSchemaName = "templateschema";
            var tenantDbContext = await _tenantDbContextResolver.GetTenantDbContextAsync(templateSchemaName);
            var createSchemaSql = $"CREATE SCHEMA `{newSchemaName}`";
            await tenantDbContext.Database.ExecuteSqlRawAsync(createSchemaSql);
            

            foreach (var table in tablesInTemplateSchema)
            {
                var copyTableSql = $"CREATE TABLE `{newSchemaName}`.`{table}` LIKE `{templateSchemaName}`.`{table}`;" +
                                   $"INSERT INTO `{newSchemaName}`.`{table}` SELECT * FROM `{templateSchemaName}`.`{table}`";
                await tenantDbContext.Database.ExecuteSqlRawAsync(copyTableSql);
            }

            tenantDbContext = await _tenantDbContextResolver.GetTenantDbContextAsync(newSchemaName);
            UserAccount user = new UserAccount
            {
                Name = newUser.Name,
                Password = newUser.Password,
                Username = newUser.Username,
                email = newUser.Email,
                phone = newUser.phone,
                roleID = 1,
                Active = true,
                roleName = "Admin",
                VerificationToken = Services.TokenGenerator.GenerateToken(),
                VerificationTokenExpiry = DateTime.UtcNow.AddHours(24),
                IsVerified = false
            };

            tenantDbContext.accounts.Add(user);
            await tenantDbContext.SaveChangesAsync();

            var verificationUrl = $"https://api.hoist.nz/api/Tenant/verify-email?token={user.VerificationToken}&databaseName={dbTenant.DatabaseName}";
            var message = $"Please verify your email by clicking <a href='{verificationUrl}'>here</a>.";
            await _emailService.SendEmailAsync(user.email, "Email Verification", message);

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.Session.SetInt32("userid", dbTenant.Id);
                httpContext.Session.SetString("sessionid", httpContext.Session.Id);
                httpContext.Session.SetString("CompanyDb", dbTenant.DatabaseName);
            }
            else
            {
                throw new InvalidOperationException("HttpContext is not available.");
            }
            
            var company = new Companies
            {
                CompanyID = dbTenant.DatabaseName,
                Credits = 0,
                PlanName = "None",
                PlanID = 1
            };
            var companiesMaster = _context.Companies.Add(company);
            await _context.SaveChangesAsync();
            return dbTenant;
        }

        private async Task<List<string>> GetTablesInTemplateSchema()
        {
            var templateSchemaName = "templateschema";
            var tenantDbContext = await _tenantDbContextResolver.GetTenantDbContextAsync(templateSchemaName);
            var tables = new List<string>();
            using (var command = tenantDbContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = $"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{templateSchemaName}'";
                tenantDbContext.Database.OpenConnection();
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

        

        public async Task VerifyEmail(string token, string databaseName)
        {
            var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync(databaseName);
            var tenant = await dbContext.accounts.FirstOrDefaultAsync(t => t.VerificationToken == token && t.VerificationTokenExpiry > DateTime.UtcNow);
            if (tenant == null)
            {
                throw new InvalidOperationException("Invalid or expired token.");
            }

            tenant.IsVerified = true;
            tenant.VerificationToken = null;
            tenant.VerificationTokenExpiry = null;

            await _context.SaveChangesAsync();
        }

        public async Task RequestPasswordReset(string email, string databaseName)
        {
            var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync(databaseName);
            var tenant = await dbContext.accounts.FirstOrDefaultAsync(t => t.email == email);
            if (tenant == null)
            {
                throw new InvalidOperationException("Email not found.");
            }

            tenant.ResetToken = Services.TokenGenerator.GenerateToken();
            tenant.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);

            await _context.SaveChangesAsync();

            var resetUrl = $"https://api.hoist.nz/api/Tenant/reset-password?token={tenant.ResetToken}";
            var message = $"You can reset your password by clicking <a href='{resetUrl}'>here</a>.";
            await _emailService.SendEmailAsync(tenant.email, "Password Reset", message);
        }

        public async Task ResetPassword(string token, string newPassword, string databaseName)
        {
            var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync(databaseName);
            
            var tenant = await dbContext.accounts.FirstOrDefaultAsync(t => t.ResetToken == token && t.ResetTokenExpiry > DateTime.UtcNow);
            if (tenant == null)
            {
                throw new InvalidOperationException("Invalid or expired token.");
            }

            
            var user = await dbContext.accounts.FirstOrDefaultAsync(u => u.email == tenant.email);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            user.Password = newPassword;

            tenant.ResetToken = null;
            tenant.ResetTokenExpiry = null;

            await dbContext.SaveChangesAsync();
            
        }

        public async Task<bool> RemoveNewTag()
        {
            
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                
                var companyID = httpContext.Session.GetString("CompanyDb");
                var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync(companyID);
                if (dbContext == null)
                {
                    throw new UnauthorizedException("just no.");
                }
                var company = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyID == companyID);

                if (company != null)
                {
                    company.New = false;
                    _context.Companies.Update(company);
                    _context.SaveChangesAsync();

                    return true;
                }
            }
            
            return false;
            
        }
    }
}
