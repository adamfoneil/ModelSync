
using EFSampleDB6.net6_0;
using System.Data.Entity;

namespace EFSampleDB.Demo
{
    public class EFSampleDBContext :DbContext
    {
        public EFSampleDBContext() : base("name=EFDemoDB")
        {
            Database.Log = Console.Write;
            Database.SetInitializer<EFSampleDBContext>(new DropCreateDatabaseAlways<EFSampleDBContext>());
        }
        public DbSet<CollegeStudent> CollegeStudent { get; set; }
        public DbSet<SchoolStudent> SchoolStudent { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Supplier> Supplier { get; set; }
        public DbSet<Employee> Employees { get; set; }

        public DbSet<Flight> Flight { get; set; }
        public DbSet<Airport> Airport { get; set; }
    }
}
