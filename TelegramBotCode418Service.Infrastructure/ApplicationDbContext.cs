using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TelegramBotCode418Service.Infrastructure.Entities;

namespace TelegramBotCode418Service.Infrastructure;

public class ApplicationDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    public ApplicationDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
        Database.EnsureCreated();
    }
    

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(_configuration.GetConnectionString("DefaultConnection"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasKey(u => u.ChatId);
    }

    public async Task<bool> IsAuthenticated(long chatId, CancellationToken cancellationToken = default)
    {
        var user = await Users
            .FirstOrDefaultAsync(u => u.ChatId == chatId.ToString(), cancellationToken);
        return user != null;
    }
    
    public DbSet<User> Users => Set<User>();
}