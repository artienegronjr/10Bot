using System;
using System.Collections.Generic;
using System.IO;
using _10Bot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace _10Bot
{
    public class EFContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Map> Maps { get; set; }

        private AppConfig _appConfig;

        public EFContext()
        {
            _appConfig = new AppConfig();
            var config = BuildConfig();

            config.GetSection("AppConfig").Bind(_appConfig);
        }

        public EFContext(IOptions<AppConfig> appConfig)
        {
            _appConfig = appConfig.Value;
        }

        private IConfigurationRoot BuildConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json")
                .Build();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_appConfig.ConnectionString);
        }
    }
}
