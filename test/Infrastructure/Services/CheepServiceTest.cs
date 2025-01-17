using Chirp.Core.DTO.CheepDTO;
using Chirp.Core.DTO.AuthorDTO;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Sdk;

namespace Chirp.Test.Infrastructure.Repositories;

public class CheepServiceTest : IDisposable
{
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Dereference of a possibly null reference.
#pragma warning disable CS8618
    private ServiceProvider _serviceProvider;
    private CheepService _cheepService;
    private CheepRepository _cheepRepository;
    private AuthorRepository _authorRepository;


    public CheepServiceTest()
    {

        var services = new ServiceCollection();

        // Using In-Memory database for testing
        services.AddDbContext<CheepDBContext>(options =>
            options.UseInMemoryDatabase("TestDbService"));

        services.AddScoped<ICheepRepository, CheepRepository>();
        services.AddScoped<IAuthorRepository, AuthorRepository>();
        services.AddScoped<ICheepService, CheepService>();

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
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);


            List<CheepDTO> list = await _cheepService.ReadPublicMessages(0);
            Assert.True(list.Count > 3);

        }

    }

    [Fact]
    public async void ReadPublicMessages()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);


            List<CheepDTO> list = await _cheepService.ReadPublicMessages(0);
            string authorName = list[0].Author;
            Boolean otherAuthor = false;

            foreach (CheepDTO item in list)
            {
                if (!item.Author.Equals(authorName))
                {
                    otherAuthor = true;
                }
            }
            // Should not be larger than the take value
            Assert.False(list.Count > 32);
            // The most recent message in the test db
            Assert.Equal("Starbuck now is what we hear the worst.", list[0].Text);
            Assert.True(otherAuthor);

        }

    }

     [Fact]
    public async void ReadPublicMessagesbyOldest()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);


            List<CheepDTO> list = await _cheepService.ReadPublicMessagesbyOldest(0);
            string authorName = list[0].Author;
            Boolean otherAuthor = false;

            foreach (CheepDTO item in list)
            {
                if (!item.Author.Equals(authorName))
                {
                    otherAuthor = true;
                }
            }
            // Should not be larger than the take value
            Assert.False(list.Count > 32);
            // The most recent message in the test db
            Assert.Equal("Hello, BDSA students!", list[0].Text);
            Assert.True(otherAuthor);

        }

    }

    [Fact]
    public async void ReadPublicMessagesbyMostLiked()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);

            await _cheepService.AddLike(1, "11");
            await _cheepService.AddLike(1, "10");
            await _cheepService.AddLike(1, "9");
            await _cheepService.AddLike(1, "8");
            await _cheepService.AddLike(1, "7");
            await _cheepService.AddLike(1, "6");
            await _cheepService.AddLike(1, "5");

            await _cheepService.AddLike(2, "11");
            await _cheepService.AddLike(2, "10");
            await _cheepService.AddLike(2, "9");
            await _cheepService.AddLike(2, "8");
            await _cheepService.AddLike(2, "7");
            await _cheepService.AddLike(2, "6");

            List<CheepDTO> list = await _cheepService.ReadPublicMessagesbyMostLiked(0);
            string authorName = list[0].Author;
            Boolean otherAuthor = false;

            foreach (CheepDTO item in list)
            {
                if (!item.Author.Equals(authorName))
                {
                    otherAuthor = true;
                }
            }



            // Should not be larger than the take value
            Assert.False(list.Count > 32);
            // The most recent message in the test db
            Assert.Equal("They were married in Chicago, with old Smith, and was expected aboard every day; meantime, the two went past me.", list[0].Text);
            Assert.Equal("And then, as he listened to all that's left o' twenty-one people.", list[1].Text);
            Assert.True(otherAuthor);

        }

    }

    [Fact]
    public async void ReadPublicMessagesbyRelevance()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);

            //659
            await _cheepRepository.CreateMessage(new NewCheepDTO
            {
                Author = "Jacqualine Gilcoine",
                AuthorId = "10",
                Text = "Hello",
                Timestamp = HelperFunctions.FromDateTimetoUnixTime(DateTime.UtcNow.AddDays(1))
            });

            //660
            await _cheepRepository.CreateMessage(new NewCheepDTO
            {
                Author = "Helge",
                AuthorId = "11",
                Text = "Hello",
                Timestamp = HelperFunctions.FromDateTimetoUnixTime(DateTime.UtcNow.AddHours(23))
            });

            //661
            await _cheepRepository.CreateMessage(new NewCheepDTO
            {
                Author = "Adrian",
                AuthorId = "12",
                Text = "Hello",
                Timestamp = HelperFunctions.FromDateTimetoUnixTime(DateTime.UtcNow.AddHours(23))
            });

            //662
            await _cheepRepository.CreateMessage(new NewCheepDTO
            {
                Author = "Johnnie Calixto",
                AuthorId = "9",
                Text = "Hello",
                Timestamp = HelperFunctions.FromDateTimetoUnixTime(DateTime.UtcNow)
            });



            await _cheepRepository.AddLike(661, "10");
            await _cheepRepository.AddLike(661, "11");
            await _cheepRepository.AddLike(661, "9");
            await _cheepRepository.AddLike(661, "8");
            await _cheepRepository.AddLike(661, "7");

            var cheep = await _cheepRepository.FindSpecificCheepbyId(659);
            var cheep2 = await _cheepRepository.FindSpecificCheepbyId(660);
            var cheep3 = await _cheepRepository.FindSpecificCheepbyId(661);
            var cheep5 = await _cheepRepository.FindSpecificCheepbyId(662);

            Assert.Equal(-25, (DateTime.UtcNow - HelperFunctions.FromUnixTimeToDateTime(cheep.Timestamp)).TotalHours, 0.5);
            Assert.Equal(-24, (DateTime.UtcNow - HelperFunctions.FromUnixTimeToDateTime(cheep2.Timestamp)).TotalHours, 0.5);

            var cheepLocalLikeRatio3 = (float)Math.Log((float)cheep3.Likes, 5);

            Assert.Equal(25, 0 - (DateTime.UtcNow - HelperFunctions.FromUnixTimeToDateTime(cheep.Timestamp)).TotalHours, 0.5);
            Assert.Equal(24, 0 - (DateTime.UtcNow - HelperFunctions.FromUnixTimeToDateTime(cheep2.Timestamp)).TotalHours, 0.5);
            // cheep 661 has gotten 2 more relevance from likes
            Assert.Equal(25, cheepLocalLikeRatio3 - (DateTime.UtcNow - HelperFunctions.FromUnixTimeToDateTime(cheep3.Timestamp)).TotalHours, 0.5);


            List<CheepDTO> list = await _cheepService.ReadPublicMessagesbyMostRelevance(0, "Helge");

            // Should not be larger than the take value
            Assert.False(list.Count > 32);
            // The most relevant
            Assert.Equal("12", list[0].AuthorId);
            Assert.Equal("10", list[1].AuthorId);
            Assert.Equal("11", list[2].AuthorId);

            await _cheepRepository.RemoveLike(661, "10");
            await _cheepRepository.RemoveLike(661, "11");
            await _cheepRepository.RemoveLike(661, "9");
            await _cheepRepository.RemoveLike(661, "8");

            await _cheepRepository.AddLike(660, "11");
            await _cheepRepository.AddLike(660, "10");

            List<CheepDTO> list2 = await _cheepService.ReadPublicMessagesbyMostRelevance(0, "Helge");

            Assert.Equal("10", list2[0].AuthorId);
            Assert.Equal("11", list2[1].AuthorId);


            // Adrian follows Helge
            await _authorRepository.AddFollower("12", "11");
            List<CheepDTO> list3 = await _cheepService.ReadPublicMessagesbyMostRelevance(0, "Adrian");

            // Helge message gets more relevance for Adrian
            Assert.Equal("11", list3[0].AuthorId);
            Assert.Equal("10", list3[1].AuthorId);

        }

    }

    [Fact]
    public async void ReadUserMessages()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);


            List<CheepDTO> list = await _cheepService.ReadUserMessages("Helge", 0);
            string authorName = list[0].Author;
            Boolean otherAuthor = false;


            foreach (CheepDTO item in list)
            {
                if (!item.Author.Equals(authorName))
                {
                    otherAuthor = true;
                    break;
                }
            }
            // Should not be larger than the take value
            Assert.False(list.Count > 32);
            // Should not show other authors on the timeline
            Assert.False(otherAuthor);
            // The authors timeline should not be empty
            Assert.True(list.Count > 0);
            Assert.Equal("Helge", authorName);

        }

    }

    [Fact]
    public async void CreateMessage()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);

            bool messageCreated = false;

            List<CheepDTO> prevList = await _cheepService.ReadUserMessages("Helge", 0);

            NewCheepDTO newMessage = new()
            {
                Author = "Helge",
                AuthorId = "11",
                Text = "I love group 13!",
                Timestamp = 12345,
            };

            await _cheepRepository.CreateMessage(newMessage);

            List<CheepDTO> newList = await _cheepService.ReadUserMessages("Helge", 0);


            foreach (CheepDTO item in newList)
            {
                if (item.Text.Equals(newMessage.Text))
                {
                    messageCreated = true;
                    break;
                }
            }
            Assert.True(newList.Count > prevList.Count);
            Assert.True(messageCreated);

        }

    }

    [Fact]
    public async void CreateMessageFail()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);

            bool messageCreated = false;

            List<CheepDTO> prevList = await _cheepService.ReadUserMessages("Helge", 0);

            NewCheepDTO newMessage = new()
            {
                Author = "Helge",
                AuthorId = "11",
                Text = "I love group 13! " +
            "I love group 13! I love group 13! I love group 13! I love group 13! I love group 13! I love group 13! I love group 13! I love group 13! I love !",
                Timestamp = 12345,
            };

            await _cheepService.CreateMessage(newMessage);

            List<CheepDTO> newList = await _cheepService.ReadUserMessages("Helge", 0);


            foreach (CheepDTO item in newList)
            {
                if (item.Text.Equals(newMessage.Text))
                {
                    messageCreated = true;
                    break;
                }
            }
            Assert.True(newList.Count == prevList.Count);
            Assert.False(messageCreated);

        }

    }


    [Fact]
    public async void CreateAuthorFromNewMessage()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);

            bool messageCreated = false;

            NewCheepDTO newMessage = new()
            {
                Author = "Helge2",
                AuthorId = "13",
                Text = "I love group 13!",
                Timestamp = 12345
            };

            await _cheepService.CreateMessage(newMessage);

            AuthorDTO author = await _cheepService.FindSpecificAuthorByName("Helge2");

            List<CheepDTO> newList = await _cheepService.ReadUserMessages("Helge2", 0);


            foreach (CheepDTO item in newList)
            {
                if (item.Text.Equals(newMessage.Text))
                {
                    messageCreated = true;
                    break;
                }
            }

            Assert.Equal("Helge2", author.Name);

            Assert.True(messageCreated);

        }

    }

    [Fact]
    public async void CreateAuthorFromNewMessageAlternative()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);

            bool messageCreated = false;

            List<CheepDTO> prevList = await _cheepService.ReadUserMessages("Helg", 0);

            NewCheepDTO newMessage = new()
            {
                Author = "Helg",
                AuthorId = "13",
                Text = "I love group 13!",
                Timestamp = 12345
            };

            await _cheepService.CreateMessage(newMessage);

            List<CheepDTO> newList = await _cheepService.ReadUserMessages("Helg", 0);


            foreach (CheepDTO item in newList)
            {
                if (item.Text.Equals(newMessage.Text))
                {
                    messageCreated = true;
                    break;
                }
            }

            Assert.True(newList.Count > prevList.Count);
            Assert.True(messageCreated);

        }
    }

    [Fact]
    public async void Follow()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);


            await _cheepService.Follow("1", "12");


            List<AuthorDTO> list = await _cheepService.GetFollowers("Roger Histand");
            List<AuthorDTO> list2 = await _cheepService.GetFollowersbyId("1");
            List<AuthorDTO> list3 = await _cheepService.GetFollowedby("Adrian");
            List<AuthorDTO> list4 = await _cheepService.GetFollowedbybyId("12");


            Assert.Equal("Adrian", list[0].Name);
            Assert.Equal("Adrian", list2[0].Name);
            Assert.Equal("Roger Histand", list3[0].Name);
            Assert.Equal("Roger Histand", list4[0].Name);


        }

    }

    [Fact]
    public async void IsFollowing()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);


            await _cheepService.Follow("1", "12");

            bool isfollowing = await _cheepService.IsFollowing("1", "12");
            Assert.True(isfollowing);
            bool isfollowing2 = await _cheepService.IsFollowing("1", "11");
            Assert.False(isfollowing2);

            bool isfollowing3 = await _cheepService.IsFollowing("12", "1");
            Assert.False(isfollowing3);
        }

    }

    [Fact]
    public async void Unfollow()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);


            await _cheepService.Follow("1", "12");


            List<AuthorDTO> list = await _cheepService.GetFollowers("Roger Histand");
            Assert.Equal("Adrian", list[0].Name);


            await _cheepService.Unfollow("1", "12");


            List<AuthorDTO> list2 = await _cheepService.GetFollowers("Roger Histand");
            Assert.False(list2.Any());

        }

    }

    [Fact]
    public async void ReadUserAndFollowerMessages()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);


            await _cheepService.Follow("11", "12");


            List<AuthorDTO> list = await _cheepService.GetFollowers("Helge");

            List<CheepDTO> listCheeps = await _cheepService.ReadUserMessages("Helge", 0);

            List<CheepDTO> listCheeps2 = await _cheepService.ReadUserAndFollowerMessages("Helge", 0);
            Assert.True(1 == list.Count);
            Assert.True(listCheeps.Count < listCheeps2.Count);

        }

    }

    [Fact]
    public async void CountUserAndFollowerMessages()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);


            await _cheepService.Follow("11", "12");


            List<AuthorDTO> list = await _cheepService.GetFollowers("Helge");

            List<CheepDTO> listCheeps = await _cheepService.ReadUserMessages("Helge", 0);
            int count1 = await _cheepService.CountUserMessages("Helge");
            int count2 = await _cheepService.CountUserMessages("Adrian");
            int count3 = await _cheepService.CountUserAndFollowerMessages("Helge");
            Assert.Equal(count1 + count2, count3);

        }

    }


    [Fact]
    public async void CountPublicMessages()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);


            int count = await _cheepService.CountPublicMessages();

            // Should be exactly 657
            Assert.Equal(658, count);
        }

    }

    [Fact]
    public async void CountUserMessages()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);


            int count = await _cheepService.CountUserMessages("Helge");

            // Should be exactly 1
            Assert.Equal(1, count);
        }

    }

    [Fact]
    public async void FindAuthorByName()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);


            List<AuthorDTO> list = await _cheepService.FindAuthorByName("Hel");
            string authorName = list[0].Name;

            Assert.Equal("Helge", authorName);
        }
    }

    [Fact]
    public async void FindAuthorByEmail()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);


            List<AuthorDTO> list = await _cheepService.FindAuthorByEmail("ropf@itu");
            string authorName = list[0].Name;

            Assert.Equal("Helge", authorName);
        }
    }

    [Fact]
    public async void FindAuthors()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);


            List<AuthorDTO> list = await _cheepService.FindAuthors("J");
            string authorName = list[0].Name;

            Assert.Equal("Jacqualine Gilcoine", list[0].Name);
            Assert.Equal("Johnnie Calixto", list[1].Name);
            Assert.Equal("Malcolm Janski", list[2].Name);


        }
    }

    [Fact]
    public async void CreateAuthor()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);

            NewAuthorDTO author = new() { Name = "Helge2", Email = "Helg2@mail.dk" };
            await _cheepService.CreateAuthor(author);
            AuthorDTO createdAuthor = await _cheepService.FindSpecificAuthorByName("Helge2");

            Assert.Equal("Helge2", createdAuthor.Name);
        }
    }

    [Fact]
    public async void FindSpecificAuthorByName()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);

            AuthorDTO author = await _cheepService.FindSpecificAuthorByName("Johnnie Calixto");
            Assert.Equal("Johnnie Calixto", author.Name);
            await Assert.ThrowsAsync<NullReferenceException>(async () => await _cheepService.FindSpecificAuthorByName("J"));

        }
    }

    [Fact]
    public async void FindSpecificAuthorById()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);

            AuthorDTO author = await _cheepService.FindSpecificAuthorById("1");
            Assert.Equal("Roger Histand", author.Name);

        }
    }

    [Fact]
    public async void FindSpecificAuthorByEmail()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);

            AuthorDTO author = await _cheepService.FindSpecificAuthorByEmail("ropf@itu.dk");
            Assert.Equal("Helge", author.Name);
            await Assert.ThrowsAsync<NullReferenceException>(async () => await _cheepService.FindSpecificAuthorByEmail("r"));
        }
    }



    [Fact]
    public async void UpdateMessage()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);

            bool messageCreated = false;


            UpdateCheepDTO newMessage = new() { Text = "I love group 13!" };
            List<CheepDTO> prevList = await _cheepService.ReadUserMessages("Helge", 0);


            await _cheepService.UpdateMessage(newMessage, 656);
            List<CheepDTO> newList = await _cheepService.ReadUserMessages("Helge", 0);


            foreach (CheepDTO item in newList)
            {
                if (item.Text.Equals(newMessage.Text))
                {
                    messageCreated = true;
                    break;
                }

            }

            Assert.True(newList.Count == prevList.Count);
            Assert.True(messageCreated);
        }
    }

    [Fact]
    public async void AddLike()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);

            //Likes own cheep (is allowed)
            await _cheepService.AddLike(656, "11");

            //Other likes same cheep
            await _cheepService.AddLike(656, "12");
            await _cheepService.AddLike(656, "12");
            await _cheepService.AddLike(656, "12");



            List<CheepDTO> cheeps = await _cheepService.ReadUserMessages("Helge", 0);
            CheepDTO cheep = await _cheepService.FindSpecificCheepbyId(656);


            Assert.Equal(2, cheeps[0].Likes);
            Assert.Equal(2, cheep.Likes);
        }
    }

    [Fact]
    public async void RemoveLike()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);

            //Likes own cheep (is allowed)
            await _cheepService.AddLike(656, "11");

            //Other likes same cheep
            await _cheepService.RemoveLike(656, "11");
            await _cheepService.RemoveLike(656, "11");


            List<CheepDTO> cheeps = await _cheepService.ReadUserMessages("Helge", 0);

            Assert.Equal(0, cheeps[0].Likes);
        }
    }

    [Fact]
    public async void HasLiked()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);

            await _cheepService.AddLike(656, "11");

            bool hasLiked = await _cheepService.HasLiked("Helge", 656);

            bool hasLiked2 = await _cheepService.HasLiked("Adrian", 656);


            Assert.True(hasLiked);
            Assert.False(hasLiked2);

            await _cheepService.RemoveLike(656, "11");
            bool hasLiked3 = await _cheepService.HasLiked("Helge", 656);

            Assert.False(hasLiked3);

            bool noSuchAuthor = await _cheepService.HasLiked("J", 656);
            //No exception was thrown
            Assert.False(noSuchAuthor);
        }
    }

    [Fact]
    public async void GetAllLikers()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);

            //Likes own cheep (is allowed)
            await _cheepService.AddLike(656, "11");

            //Other likes same cheep
            await _cheepService.AddLike(656, "12");
            await _cheepService.AddLike(656, "12");
            await _cheepService.AddLike(656, "12");



            List<AuthorDTO> authors = await _cheepService.GetAllLikers(656);
            Assert.Equal(2, authors.Count);
            Assert.Equal("Helge", authors[0].Name);
        }
    }

    [Fact]
    public async void AddDisLike()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);

            //Dislikes own cheep (is allowed)
            await _cheepService.AddDislike(656, "11");

            //Other dislikes same cheep
            await _cheepService.AddDislike(656, "12");
            await _cheepService.AddDislike(656, "12");
            await _cheepService.AddDislike(656, "12");

            List<CheepDTO> cheeps = await _cheepService.ReadUserMessages("Helge", 0);
            CheepDTO cheep = await _cheepService.FindSpecificCheepbyId(656);


            Assert.Equal(2, cheeps[0].Dislikes);
            Assert.Equal(2, cheep.Dislikes);
        }
    }

    [Fact]
    public async void RemoveDislike()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);

            //dislikes own cheep (is allowed)
            await _cheepService.AddDislike(656, "11");

            //Removes dislike for same user
            await _cheepService.RemoveDislike(656, "11");
            await _cheepService.RemoveDislike(656, "11");


            List<CheepDTO> cheeps = await _cheepService.ReadUserMessages("Helge", 0);

            Assert.Equal(0, cheeps[0].Dislikes);
        }
    }

    [Fact]
    public async void HasDisliked()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);

            await _cheepService.AddDislike(656, "11");

            bool HasDisliked = await _cheepService.HasDisliked("Helge", 656);

            bool HasDisliked2 = await _cheepService.HasDisliked("Adrian", 656);


            Assert.True(HasDisliked);
            Assert.False(HasDisliked2);

            await _cheepService.RemoveDislike(656, "11");
            bool HasDisliked3 = await _cheepService.HasDisliked("Helge", 656);

            Assert.False(HasDisliked3);


            bool noSuchAuthor = await _cheepService.HasDisliked("J", 656);
            //No exception was thrown
            Assert.False(noSuchAuthor);

        }
    }

    

        [Fact]
    public async void GetAllDislikers()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);

            //Likes own cheep (is allowed)
            await _cheepService.AddDislike(656, "11");

            //Other likes same cheep
            await _cheepService.AddDislike(656, "12");
            await _cheepService.AddDislike(656, "12");
            await _cheepService.AddDislike(656, "12");



            List<AuthorDTO> authors = await _cheepService.GetAllDislikers(656);
            Assert.Equal(2, authors.Count);
            Assert.Equal("Helge", authors[0].Name);

            List<AuthorDTO> authorLikes = await _cheepService.GetAllLikers(656);
            Assert.Empty(authorLikes);

        }
    }

     [Fact]
    public async void ForgetMe()
    {
        using (var scope = _serviceProvider.CreateScope())
        {

            var context = scope.ServiceProvider.GetService<CheepDBContext>();
            _cheepRepository = new CheepRepository(context);
            _authorRepository = new AuthorRepository(context);
            _cheepService = new CheepService(_cheepRepository, _authorRepository);


            await _cheepService.Follow("11", "12");

            await _cheepService.Follow("12", "11");


            await _cheepService.AddLike(656, "11");
            await _cheepService.AddDislike(656, "11");

            await _cheepService.ForgetMe("Helge");
            List<AuthorDTO> listFollowers = await _cheepService.GetFollowedby("Adrian");

            List<CheepDTO> listCheeps = await _cheepService.ReadUserMessages("Helge", 0);

            Assert.Empty(listFollowers);
            Assert.Empty(listCheeps);

            try
            {
                await _cheepService.GetAllLikers(656);
                Assert.Fail();
            }
            catch (System.Exception) {}
            try
            {
                await _cheepService.GetAllDislikers(656);
                Assert.Fail();
            }
            catch (System.Exception){}

            try{
                await _cheepService.ForgetMe("Helge");
                Assert.Fail();
            }
            catch (System.Exception) {}

        }

    }
}