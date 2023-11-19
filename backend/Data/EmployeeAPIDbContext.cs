using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    public class EmployeeAPIDbContext : DbContext
    {
        public EmployeeAPIDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<EmployeeDetails> EmployeeDetails { get; set; }
    }
}