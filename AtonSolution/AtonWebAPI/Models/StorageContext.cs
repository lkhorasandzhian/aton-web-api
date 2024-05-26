using Microsoft.EntityFrameworkCore;

namespace AtonWebAPI.Models
{
	public class StorageContext : DbContext
	{
		public StorageContext(DbContextOptions<StorageContext> options) : base(options) { }

		public DbSet<User> Users { get; set; } = null!;
	}
}
