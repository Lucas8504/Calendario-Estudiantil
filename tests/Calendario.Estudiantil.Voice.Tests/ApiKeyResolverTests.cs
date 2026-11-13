using Calendario_Estudiantil.Services.Voice;
using Xunit;

namespace Calendario_Estudiantil.Voice.Tests;

public class ApiKeyResolverTests
{
    [Fact]
    public void StoredKey_WinsOverEnvironment()
    {
        var key = ApiKeyResolver.Resolve("stored-key", "env-key");

        Assert.Equal("stored-key", key);
    }

    [Fact]
    public void EmptyStoredKey_FallsBackToEnvironment()
    {
        var key = ApiKeyResolver.Resolve("   ", "env-key");

        Assert.Equal("env-key", key);
    }

    [Fact]
    public void MissingStoredKey_FallsBackToEnvironment()
    {
        var key = ApiKeyResolver.Resolve(null, "env-key");

        Assert.Equal("env-key", key);
    }

    [Fact]
    public void WhitespaceOnlySources_ReturnNull()
    {
        var key = ApiKeyResolver.Resolve("  ", "   ");

        Assert.Null(key);
    }

    [Fact]
    public void NullSources_ReturnNull()
    {
        var key = ApiKeyResolver.Resolve(null, null);

        Assert.Null(key);
    }

    [Fact]
    public void KeysAreTrimmed()
    {
        var key = ApiKeyResolver.Resolve("  stored-key  ", null);

        Assert.Equal("stored-key", key);
    }
}
