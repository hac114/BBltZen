using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<BubbleTeaContext
            > options) : base(options)
        {
        }
        public DbSet<DolceDTO> Dolci { get; set; }
        public DbSet<OrdineDTO> Ordini { get; set; }
    }
}
