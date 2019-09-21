using Microsoft.EntityFrameworkCore;

namespace Nenter.Data.EntityFramework
{
    public  class MyContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>()
                .Property(b => b.Url)
                .IsRequired();

            modelBuilder.Entity<Blog>().ToTable("");
            // 禁用筛选器 IgnoreQueryFilters() 

            //modelBuilder.Entity<Blog>().Property<string>("TenantId").HasField("_tenantId");

            // Configure entity filters
            //modelBuilder.Entity<Blog>().HasQueryFilter(b => EF.Property<string>(b, "TenantId") == _tenantId);
            //modelBuilder.Entity<Blog>().HasQueryFilter(p => !p.IsDeleted);
        }
    }

    public class Blog
    {
        private string _tenantId;
        
        public int BlogId { get; set; }
        public string Url { get; set; }
        
        public bool IsDeleted { get; set; }
    }
}