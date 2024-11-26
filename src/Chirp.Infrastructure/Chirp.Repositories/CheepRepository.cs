using Microsoft.EntityFrameworkCore;
using Chirp.Core.DTO;
using Chirp.Core.Entities;

namespace Chirp.Repositories;


public class CheepRepository(CheepDBContext dbContext) : ICheepRepository
{

    private readonly CheepDBContext _dbContext = dbContext;
    public async Task<int> CreateMessage(CheepDTO message)
    {

        Cheep newCheep = new() { Text = message.Text, AuthorId = message.AuthorId, TimeStamp = HelperFunctions.FromUnixTimeToDateTime(message.Timestamp) };
        var queryResult = await _dbContext.Cheeps.AddAsync(newCheep); // does not write to the database!

        await _dbContext.SaveChangesAsync(); // persist the changes in the database
        return queryResult.Entity.CheepId;
    }

    public async Task<List<CheepDTO>> ReadPublicMessages(int takeValue, int skipValue)
    {

        // Formulate the query - will be translated to SQL by EF Core
        var query = _dbContext.Cheeps.OrderByDescending(message => message.TimeStamp)
        .Skip(skipValue)
        .Take(takeValue)
        .Select(message => new CheepDTO
        {
            AuthorId = message.AuthorId,
            Author = message.Author.UserName!,
            Text = message.Text,
            Timestamp = ((DateTimeOffset)message.TimeStamp).ToUnixTimeMilliseconds()
        });
        // Execute the query
        var result = await query.ToListAsync();

        return result;
    }

    public async Task<List<CheepDTO>> ReadUserMessages(string userName, int takeValue, int skipValue)
    {
        // Formulate the query - will be translated to SQL by EF Core
        var query = _dbContext.Cheeps.OrderByDescending(message => message.TimeStamp)
        .Where(message => message.Author.UserName == userName)
        .Skip(skipValue)
        .Take(takeValue)
        .Select(message => new CheepDTO
        {
            AuthorId = message.AuthorId,
            Author = message.Author.UserName!,
            Text = message.Text,
            Timestamp = ((DateTimeOffset)message.TimeStamp).ToUnixTimeMilliseconds()
        });
        // Execute the query
        var result = await query.ToListAsync();
        return result;
    }

    public async Task<List<CheepDTO>> ReadUserAndFollowerMessages(string userName, List<string> followers, int takeValue, int skipValue)
    {
        // Formulate the query - will be translated to SQL by EF Core
        var query = _dbContext.Cheeps.OrderByDescending(message => message.TimeStamp)
        .Where(message => message.Author.UserName == userName || followers.Contains(message.Author.UserName!))
        .Skip(skipValue)
        .Take(takeValue)
        .Select(message => new CheepDTO
        {
            AuthorId = message.AuthorId,
            Author = message.Author.UserName!,
            Text = message.Text,
            Timestamp = ((DateTimeOffset)message.TimeStamp).ToUnixTimeMilliseconds()
        });
        // Execute the query
        var result = await query.ToListAsync();
        return result;
    }




    public async Task UpdateMessage(CheepDTO alteredMessage, int id)
    {

        var cheep = _dbContext.Cheeps.Single(e => e.CheepId == id);
        var entityEntry = _dbContext.Entry(cheep);
        _dbContext.Entry(cheep).CurrentValues.SetValues(alteredMessage);

        await _dbContext.SaveChangesAsync(); // persist the changes in the database
        return;
    }

    public async Task RemoveCheepsFromUser(string userName)
    {

        var cheeps = _dbContext.Cheeps.Where(c => c.Author.UserName == userName);
        _dbContext.RemoveRange(cheeps);
        await _dbContext.SaveChangesAsync(); // persist the changes in the database
        return;

    }
}