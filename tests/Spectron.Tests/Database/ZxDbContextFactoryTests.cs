using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OldBit.Spectron.Database;
using OldBit.Spectron.Database.Data;
using OldBit.Spectron.Database.DependencyInjection;

namespace OldBit.Spectron.Tests.Database;

public class ZxDbContextFactoryTests
{
    [Fact]
    public void AddZxDb_ResolvesServices_WithoutDatabasePath()
    {
        var services = new ServiceCollection();
        services.AddZxDb();

        using var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<ZxDbClient>();

        client.IsAvailable.ShouldBeFalse();
        provider.GetRequiredService<IDbContextFactory<GamesDbContext>>().ShouldBeOfType<ZxDbContextFactory>();
    }

    [Fact]
    public void CreateDbContext_ShouldThrow_WhenPathNotSet()
    {
        var factory = new ZxDbContextFactory(new ZxDbSettings());

        Should.Throw<ZxDbNotConfiguredException>(() => factory.CreateDbContext());
    }

    [Fact]
    public void CreateDbContext_ShouldThrow_WhenFileDoesNotExist()
    {
        var settings = new ZxDbSettings { DatabasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.sqlite") };
        var factory = new ZxDbContextFactory(settings);

        Should.Throw<ZxDbNotConfiguredException>(() => factory.CreateDbContext());
    }

    [Fact]
    public void CreateDbContext_ShouldUseCurrentPath_WhenPathChanges()
    {
        using var first = ZxDbTestDatabase.Create();
        using var second = ZxDbTestDatabase.Create();

        var settings = new ZxDbSettings { DatabasePath = first.Path };
        var factory = new ZxDbContextFactory(settings);

        using (var db = factory.CreateDbContext())
        {
            db.Database.GetConnectionString()!.ShouldContain(first.Path);
        }

        settings.DatabasePath = second.Path;

        using (var db = factory.CreateDbContext())
        {
            db.Database.GetConnectionString()!.ShouldContain(second.Path);
            db.Database.GetConnectionString()!.ShouldContain("Mode=ReadOnly");
        }
    }

    [Fact]
    public async Task IsValidDatabase_ShouldReturnTrue_ForZxDbSchema()
    {
        using var database = ZxDbTestDatabase.Create();

        var isValid = await ZxDbClient.IsValidDatabaseAsync(database.Path, TestContext.Current.CancellationToken);

        isValid.ShouldBeTrue();
    }

    [Fact]
    public async Task IsValidDatabase_ShouldReturnFalse_ForOtherSchema()
    {
        using var database = ZxDbTestDatabase.Create(isValidSchema: false);

        var isValid = await ZxDbClient.IsValidDatabaseAsync(database.Path, TestContext.Current.CancellationToken);

        isValid.ShouldBeFalse();
    }

    [Fact]
    public async Task IsValidDatabase_ShouldReturnFalse_ForNonDatabaseFile()
    {
        var path = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(path, "not a database", TestContext.Current.CancellationToken);

            var isValid = await ZxDbClient.IsValidDatabaseAsync(path, TestContext.Current.CancellationToken);

            isValid.ShouldBeFalse();
        }
        finally
        {
            File.Delete(path);
        }
    }
}
