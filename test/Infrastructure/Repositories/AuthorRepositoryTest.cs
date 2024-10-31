using Chirp.Core.DTO;
using Chirp.Core.Entities;
using Chirp.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit.Sdk;

namespace Repositories;

public class AuthorRepositoryTest : IDisposable
{
    #pragma warning disable CS8602 // Dereference of a possibly null reference.
    #pragma warning disable CS8604 // Dereference of a possibly null reference.

    private ServiceProvider _serviceProvider;
    

    public AuthorRepositoryTest(){

        var services = new ServiceCollection();

        // Using In-Memory database for testing: Note that the inmemorydatabse is different from other test classes
        services.AddDbContext<CheepDBContext>(options =>
            options.UseInMemoryDatabase("TestDb2"));
        services.AddScoped<AuthorRepository>();

        _serviceProvider = services.BuildServiceProvider();

        using (var scope = _serviceProvider.CreateScope())
        {
            // From the scope, get an instance of our database context.
            // Through the `using` keyword, we make sure to dispose it after we are done.
            using var context = scope.ServiceProvider.GetService<CheepDBContext>();
            // Execute the migration from code.
            try
            {
                context.Database.Migrate();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            DbInitializer.SeedDatabase(context);
        }
    }

    public void Dispose()
    {
        var dbContext = _serviceProvider.GetService<CheepDBContext>();
        dbContext.Database.EnsureDeleted();
    }

    [Fact]
    public async void SeededDatabase()
    {
        using (var scope = _serviceProvider.CreateScope()){

            using (var context = scope.ServiceProvider.GetService<CheepDBContext>()){
                var repo = new CheepRepository(context);

                List<CheepDTO> list = await repo.ReadPublicMessages(32, 0);
                Assert.True(list.Count > 3);
            }
        }
    }

    [Fact]
    public async void FindAuthorByName()
    {
        using (var scope = _serviceProvider.CreateScope()){

            using (var context = scope.ServiceProvider.GetService<CheepDBContext>()){
                var repo = new AuthorRepository(context);

                List<AuthorDTO> authors = await repo.FindAuthorByName("Helge");

                // Should not be larger than the take value
                Assert.False(authors.Count > 1);
                // The most recent message in the test db
                Assert.Equal("Helge", authors[0].Name);
            }
        }
    }

    [Fact]
    public async void FindMultipleAuthors()
    {
        using (var scope = _serviceProvider.CreateScope()){

            using (var context = scope.ServiceProvider.GetService<CheepDBContext>()){
                var repo = new AuthorRepository(context);

                List<AuthorDTO> authors = await repo.FindAuthorByName("J");

                // The most recent message in the test db
                Assert.Equal("Jacqualine Gilcoine", authors[0].Name);
                Assert.Equal("Johnnie Calixto", authors[1].Name);
            }
        }
    }
    
    [Fact]
    public async void FindAuthorByEmail()
    {
        using (var scope = _serviceProvider.CreateScope()){

            using (var context = scope.ServiceProvider.GetService<CheepDBContext>()){
                var repo = new AuthorRepository(context);

                List<AuthorDTO> authors = await repo.FindAuthorByEmail("ropf@itu.dk");

                // Should not be larger than the take value
                Assert.False(authors.Count > 1);
                // The most recent message in the test db
                Assert.Equal("Helge", authors[0].Name);
            }
        }
    }

   

}