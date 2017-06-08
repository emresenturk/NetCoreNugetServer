using Microsoft.EntityFrameworkCore;

namespace NugetServer.Models
{
	public class ApplicationDataContext : DbContext
	{
		public ApplicationDataContext(DbContextOptions<ApplicationDataContext> options) : base(options)
		{
		}

		public virtual DbSet<Package> Packages { get; set; }
	}
}
