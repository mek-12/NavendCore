using Microsoft.EntityFrameworkCore;

namespace Navend.Core.Data.EfCore;

/// <summary>
/// This db context must be use with select commands. Like Find, Where ext.
/// </summary>
public class NoLockDbContext : DbContext {
    
    public NoLockDbContext() {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.AddInterceptors(new NoLockCommandInterceptor());
    }
}