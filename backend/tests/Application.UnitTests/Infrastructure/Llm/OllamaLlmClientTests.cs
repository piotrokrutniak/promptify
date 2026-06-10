using System.Net;
using PromptifyWebApi.Application.Common.Llm;
using PromptifyWebApi.Infrastructure.Llm;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Shouldly;

namespace PromptifyWebApi.Application.UnitTests.Infrastructure.Llm;

public class OllamaLlmClientTests
{
    [Test]
    public async Task CompleteAsync_ShouldPostChatRequestAndReturnAssistantContent()
    {
        var handler = new StubHttpMessageHandler(
            request =>
            {
                request.Method.ShouldBe(HttpMethod.Post);
                request.RequestUri!.AbsolutePath.ShouldBe("/api/chat");

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                          "message": {
                            "role": "assistant",
                            "content": "hello from ollama"
                          }
                        }
                        """),
                };
            });

        var httpClientFactory = new StubHttpClientFactory(
            new HttpClient(handler) { BaseAddress = new Uri("http://localhost:11434/") });

        var client = new OllamaLlmClient(
            httpClientFactory,
            Options.Create(new LlmOptions
            {
                OllamaModel = "llama3.2",
            }));

        var result = await client.CompleteAsync(
            [new LlmMessage(LlmRole.User, "hi")],
            CancellationToken.None);

        result.ShouldBe("hello from ollama");
        handler.RequestCount.ShouldBe(1);
        httpClientFactory.CreateClientCallCount.ShouldBe(1);
    }

    [Test]
    public async Task CompleteAsync_OnEachCall_ShouldCreateClientFromFactory()
    {
        var httpClientFactory = new StubHttpClientFactory(
            new HttpClient(new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                      "message": {
                        "role": "assistant",
                        "content": "ok"
                      }
                    }
                    """),
            }))
            {
                BaseAddress = new Uri("http://localhost:11434/"),
            });

        var client = new OllamaLlmClient(
            httpClientFactory,
            Options.Create(new LlmOptions { OllamaModel = "llama3.2" }));

        await client.CompleteAsync([new LlmMessage(LlmRole.User, "one")], CancellationToken.None);
        await client.CompleteAsync([new LlmMessage(LlmRole.User, "two")], CancellationToken.None);

        httpClientFactory.CreateClientCallCount.ShouldBe(2);
        httpClientFactory.LastClientName.ShouldBe(OllamaLlmClient.HttpClientName);
    }

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _httpClient;

        public StubHttpClientFactory(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public int CreateClientCallCount { get; private set; }

        public string? LastClientName { get; private set; }

        public HttpClient CreateClient(string name)
        {
            CreateClientCallCount++;
            LastClientName = name;
            return _httpClient;
        }
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;
            return Task.FromResult(_handler(request));
        }
    }
}
