using GestaoDeTarefas.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace GestaoDeTarefas.Api.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TodoTask> Tasks => Set<TodoTask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var task = modelBuilder.Entity<TodoTask>();

        task.HasKey(t => t.Code);
        task.Property(t => t.Code).HasMaxLength(20);
        task.Property(t => t.Title).HasMaxLength(200).IsRequired();
        task.Property(t => t.Description).HasMaxLength(1000);
        task.Property(t => t.Status).HasConversion<string>();
    }
}
