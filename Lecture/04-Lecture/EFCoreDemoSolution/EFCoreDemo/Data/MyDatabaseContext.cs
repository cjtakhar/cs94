using EFCoreDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCoreDemo.Data
{
    /// <summary>
    /// Coordinates Entity Framework functionality for a given data model is the database context class
    /// </summary>
    /// <seealso cref="Microsoft.EntityFrameworkCore.DbContext" />
    /// <remarks>Step 6</remarks>
    public class MyDatabaseContext : DbContext
    {
        // Note: To open the DB in sql management studio either:
        // Use: (LocalDb)\mssqllocaldb
        //
        // Or obtain the connection using
        // C:\Program Files\Microsoft SQL Server\140\Tools\Binn>sqllocaldb info mssqllocaldb
        // Copy the Instance pipe name: 
        // Ex: np:\\.\pipe\LOCALDB#B40AAB9D\tsql\query 

        /// <summary>
        /// Initializes a new instance of the <see cref="MyDatabaseContext"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <remarks>Step 6a</remarks>
        public MyDatabaseContext(DbContextOptions<MyDatabaseContext> options) : base(options)
        {
            // DEMO: This will cause the Database and Tables to be created.
            //       ONLY IF THERE ARE NO TABLES IN THE DATABASE
            //       IF THE DATABASE CONTAINS TABLES THEN THIS METHOD WILL NOT CREATE ANY TABLES
            //       EVEN IF THOSE TABLES ARE NOT IN THE DATABASE
            Database.EnsureCreated();
        }

        /// <summary>
        /// Represents the Customers table (Entity Set)
        /// </summary>
        /// <value>
        /// The customers.
        /// </value>
        /// <remarks>Step 6b</remarks>
        public DbSet<Customer> Customers { get; set; }

        /// <summary>
        /// Represents the Addresses table (Entity Set)
        /// </summary>
        /// <value>
        /// The addresses.
        /// </value>
        public DbSet<Address> Addresses { get; set; }

        /// <summary>
        /// Override this method to further configure the model that was discovered by convention from the entity types
        /// exposed in <see cref="T:Microsoft.EntityFrameworkCore.DbSet`1" /> properties on your derived context. The resulting model may be cached
        /// and re-used for subsequent instances of your derived context.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context. Databases (and other extensions) typically
        /// define extension methods on this object that allow you to configure aspects of the model that are specific
        /// to a given database.</param>
        /// <remarks>
        /// Step 6c
        /// If a model is explicitly set on the options for this context (via <see cref="M:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel)" />)
        /// then this method will not be run.
        /// </remarks>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Adds the Customer to the entity model linking it to the Customer table
            modelBuilder.Entity<Customer>().ToTable("Customer")
                .HasMany(c => c.Addresses)
                .WithOne(t => t.Customer)
                .HasForeignKey(t => t.CustomerId);
        }

        /// <summary>
        /// Configure enhanced logging
        /// </summary>
        /// <param name="optionsBuilder">The operation builder</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
        }
    }


}

