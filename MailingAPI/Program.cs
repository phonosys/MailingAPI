using MailingAPI.Models;
using Microsoft.EntityFrameworkCore;

#region WebApp Builder and Vars
Dictionary<int, TrackingStatus> CachedPackageStatusItems = new Dictionary<int, TrackingStatus>();
HashSet<Package> CachedPackages = new HashSet<Package>();
var builder = WebApplication.CreateBuilder(args);
var dbContextOptions = new DbContextOptionsBuilder<MailingDB>()
     .UseInMemoryDatabase("PostOffice")
     .Options;
var dbContext = new MailingDB(dbContextOptions);
builder.Services.AddDbContext<MailingDB>(opt => opt.UseInMemoryDatabase("PostOffice"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();
#endregion

#region Post Office API
/// <summary>
///  Post Office API
/// </summary>
app.MapGet("/po", async (MailingDB db) =>
    await db.PostOffices.ToListAsync());

//app.MapGet("/po/complete", async (MailingDB db) =>
//    await db.PostOffices.Where(t => t.IsComplete).ToListAsync());

app.MapGet("/po/{id}", async (int id, MailingDB db) =>
    await db.PostOffices.FindAsync(id)
        is PostOffice po
            ? Results.Ok(po)
            : Results.NotFound());

app.MapPost("/po", async (PostOffice _po, MailingDB db) =>
{
    db.PostOffices.Add(_po);
    await db.SaveChangesAsync();
    return Results.Created($"/po/{_po.Id}", _po);
});

app.MapPut("/po/{id}", async (int id, PostOffice _po, MailingDB db) =>
{
    var po = await db.PostOffices.FindAsync(id);
    if (po is null) return Results.NotFound();
    po.Name = _po.Name;
    po.Address = _po.Address;
    po.ZipCode = _po.ZipCode;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/po/{id}", async (int id, MailingDB db) =>
{
    if (await db.PostOffices.FindAsync(id) is PostOffice po)
    {
        db.PostOffices.Remove(po);
        await db.SaveChangesAsync();
        return Results.Ok(po);
    }

    return Results.NotFound();
});
#endregion

#region Packages API
/// <summary>
///  Post Office API
/// </summary>
app.MapGet("/pkg", async (MailingDB db) =>
    await db.Packages.ToListAsync());

app.MapGet("/pkg/{trackingNumber}", async (string trackingNumber, MailingDB db) =>
    await db.Packages.FirstOrDefaultAsync(x => x.TrackingNumber == trackingNumber)
        is Package pkg
            ? Results.Ok(pkg.TrackingHistory.Count == 0 ? "No Tracking Information" : pkg.TrackingNumber)
            : Results.NotFound("Package not found."));

app.MapPost("/pkg", async (Package _pkg, MailingDB db) =>
{
    db.Packages.Add(_pkg);
    await db.SaveChangesAsync();
    return Results.Created($"/pkg/{_pkg.Id}", _pkg);
});


app.MapPost("/pkg/{id}/{status}", async (int id, TrackingStatus status, MailingDB db) =>
{
    var pkg = await db.Packages.FindAsync(id);
    if(pkg == null)  
        return Results.NotFound();
    CachedPackageStatusItems.Add(pkg.Id, status);
    return Results.Ok();
});

app.MapPut("/pkg/{id}", async (int id, PostOffice _po, MailingDB db) =>
{
    var po = await db.PostOffices.FindAsync(id);
    if (po is null) return Results.NotFound();
    po.Name = _po.Name;
    po.Address = _po.Address;
    po.ZipCode = _po.ZipCode;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/pkg/{id}", async (int id, MailingDB db) =>
{
    if (await db.PostOffices.FindAsync(id) is PostOffice po)
    {
        db.PostOffices.Remove(po);
        await db.SaveChangesAsync();
        return Results.Ok(po);
    }
    return Results.NotFound();
});
#endregion

#region Periodic Tasks
async Task UpdateCachedItemsStatus()
{
    foreach (var item in CachedPackageStatusItems)
    {
        var pkg = dbContext.Packages.FirstOrDefault(x => x.Id == item.Key);
        if (pkg is null) continue;
        pkg.TrackingHistory.Add(new TrackingHistory()
        {
            EventTime = DateTime.Now,
            Status = item.Value
        });
        CachedPackageStatusItems.Remove(item.Key);
    }
    await dbContext.SaveChangesAsync();
}
async Task RegisterCachedPackages()
{
    foreach (var pkg in CachedPackages)
    {
        dbContext.Packages.Add(pkg);
        CachedPackages.Remove(pkg);
    }
    await dbContext.SaveChangesAsync();
}
/// <summary>
/// Periodically updating packages statuses
/// </summary>
new Timer(
    async e => await UpdateCachedItemsStatus(),
    null,
    TimeSpan.Zero,
    TimeSpan.FromMinutes(3));

/// <summary>
/// Periodically Registering Packages
/// </summary>
new Timer(
    async e => await RegisterCachedPackages(),
    null,
    TimeSpan.Zero,
    TimeSpan.FromMinutes(1));
#endregion

app.MapGet("/", () => "PostOfficeAPI");
app.Run();

class MailingDB : DbContext
{
    public MailingDB(DbContextOptions<MailingDB> options)
        : base(options) {
    }

    public DbSet<PostOffice> PostOffices => Set<PostOffice>();
    public DbSet<Package> Packages => Set<Package>();

}