using BankSim.Models.Database.Tables;
using Microsoft.EntityFrameworkCore;

namespace BankSim.Models.Database
{
    public class BankSimDbContext : DbContext
    {
        public BankSimDbContext(){}
        public BankSimDbContext(DbContextOptions<BankSimDbContext> options)
            : base(options){}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

                optionsBuilder.UseSqlServer(configuration.GetConnectionString("ConnStr"));
            }
        }


        public DbSet<CustomerTb> CustomerTb { get; set; }
        public DbSet<AccountTb> AccountTb { get; set; }
        public DbSet<TransactionTb> TransactionTb { get; set; }
    }
}
