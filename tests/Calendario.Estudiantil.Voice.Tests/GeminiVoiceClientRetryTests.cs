using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;

using Calendario_Estudiantil.Models;
using Calendario_Estudiantil.Services.Voice;

using Xunit;

namespace Calendario_Estudiantil.Voice.Tests;

/// <summary>
/// Reintentos ante fallos de transporte (IOException/SocketException) en GeminiVoiceClient.
/// Un reset de socket a mitad de la request es transitorio: debe reintentar como
/// ya se hacía con HttpRequestException, no cortar de una al usuario.
/// </summary>
public class GeminiVoiceClientRetryTests
{
    private const string SuccessJson =
        """{"candidates":[{"content":{"parts":[{"text":"ok"}]}}]}""";

    [Fact]
    public async Task TransportFailureOnce_RetriesAndSucceeds()
    {
        var handler = new ScriptedHandler(
            _ => throw new IOException("Connection reset by peer"),
            _ => Ok());
        var client = CreateClient(handler);

        var result = await client.GetVoiceCommandsAsync(
            Stream.Null, new List<FunctionDeclaration>(), DateTime.Now);

        Assert.NotNull(result);
        Assert.Equal("ok", result.ModelResponseText);
        Assert.Equal(2, handler.Calls);
    }

    [Fact]
    public async Task TransportFailureAlways_PropagatesAfterExhaustingRetries()
    {
        var handler = new ScriptedHandler(
            _ => throw new IOException("Connection reset by peer"));
        var client = CreateClient(handler);

        var ex = await Assert.ThrowsAsync<IOException>(() =>
            client.GetVoiceCommandsAsync(
                Stream.Null, new List<FunctionDeclaration>(), DateTime.Now));

        Assert.Contains("Connection reset", ex.Message);
        // 1 intento inicial + MaxRetries (3) reintentos = 4 llamadas al transporte.
        Assert.Equal(4, handler.Calls);
    }

    [Fact]
    public void IsTransportFailure_ClassifiesSocketAndIoButNotOtherErrors()
    {
        Assert.True(GeminiVoiceClient.IsTransportFailure(
            new SocketException(10054)));
        Assert.True(GeminiVoiceClient.IsTransportFailure(
            new IOException("Connection reset by peer")));
        Assert.False(GeminiVoiceClient.IsTransportFailure(
            new InvalidOperationException("boom")));
    }

    private static GeminiVoiceClient CreateClient(HttpMessageHandler handler)
    {
        var client = new GeminiVoiceClient(new HttpClient(handler), new FakeApiKeyStore());
        client.RetryDelay = _ => 0; // sin espera real: el backoff de producción no se testea aquí
        return client;
    }

    private static HttpResponseMessage Ok()
        => new(HttpStatusCode.OK)
        {
            Content = new StringContent(SuccessJson, Encoding.UTF8, "application/json")
        };

    private sealed class FakeApiKeyStore : IApiKeyStore
    {
        public Task<string?> GetGeminiApiKeyAsync() => Task.FromResult<string?>("test-key");

        public Task SetGeminiApiKeyAsync(string key) => Task.CompletedTask;

        public Task ClearGeminiApiKeyAsync() => Task.CompletedTask;
    }

    /// <summary>Handler guionizado: cada paso recibe el índice de llamada (0-based).</summary>
    private sealed class ScriptedHandler : HttpMessageHandler
    {
        private readonly Func<int, HttpResponseMessage>[] _steps;

        public int Calls { get; private set; }

        public ScriptedHandler(params Func<int, HttpResponseMessage>[] steps)
            => _steps = steps;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var step = _steps[Math.Min(Calls, _steps.Length - 1)];
            var index = Calls;
            Calls++;
            return Task.FromResult(step(index));
        }
    }
}
