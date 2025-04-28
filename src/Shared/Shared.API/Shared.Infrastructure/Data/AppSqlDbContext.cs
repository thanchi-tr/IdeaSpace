
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Models;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Shared.Infrastructure.Data
{
    public class AppSqlDbContext : DbContext
    {
        public AppSqlDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Collection> Collections { get; set; }
        public DbSet<CollectionExpiredIdea> CollectionExpiredIdeas { get; set; }
        public DbSet<CollectionIdea> CollectionIdeas { get; set; }
        public DbSet<ExpiredIdea> ExpiredIdeas { get; set; }
        public DbSet<Idea> ideas { get; set; }
        public DbSet<Label> Labels { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TagExpiredIdea> TagExpiredIdeas { get; set; }
        public DbSet<TagIdea> TagIdeas { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Apply configurations dynamically 
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppSqlDbContext).Assembly);
            // Set name to convention
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var p in entity.GetProperties())
                {
                    p.SetColumnName(ToSnakeCase(p.Name));
                }
            }
            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// @Todo: refractorise this away later
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static readonly Regex AcronymsRegex = new Regex(
            @"([A-Z]+)([A-Z][a-z])",
            RegexOptions.Compiled,
            TimeSpan.FromSeconds(5));

        private static readonly Regex CamelCaseRegex = new Regex(
            @"([a-z0-9])([A-Z])",
            RegexOptions.Compiled,
            TimeSpan.FromSeconds(5));

        private string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var result = AcronymsRegex.Replace(input, "$1_$2");
            result = CamelCaseRegex.Replace(result, "$1_$2");
            return result.ToLower(CultureInfo.InvariantCulture);
        }
    }
}
