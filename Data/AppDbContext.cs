using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data;
// via Entity framework
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Membre> Membres { get; set; } = null!;
    public DbSet<Abonnement> Abonnements { get; set; } = null!;
    public DbSet<Paiement> Paiements { get; set; } = null!;
    public DbSet<Admin> Admins { get; set; } = null!;
}
