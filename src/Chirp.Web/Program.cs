using Chirp.Services;
using Chirp.Repositories;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<CheepDBContext>(options => options.UseSqlite(connectionString));

var filePath = "./data/chirps.db";

if (!File.Exists(filePath))
{

    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
    using (var fs = File.Create(filePath))
    {
        fs.Close();
        Console.WriteLine($"File {filePath} created.");
    }
}


if (File.Exists(filePath))
{
    using var reader = new FileStream(filePath, FileMode.Open, FileAccess.Read);
    using var sr = new StreamReader(reader);
    var query = sr.ReadToEnd();

    var i = 0;
    foreach (var queri in query)
    {
        Console.WriteLine(++i);
        Console.WriteLine(queri);
    }
}
else
{
    Console.WriteLine("File not found.");
}


builder.Services.AddRazorPages();
builder.Services.AddSingleton<ICheepService, CheepService>();
builder.Services.AddScoped<ICheepRepository, CheepRepository>();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

app.Run();