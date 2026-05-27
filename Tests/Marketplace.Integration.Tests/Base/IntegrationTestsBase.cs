using Marketplace.Repository.MSSql;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using Xunit;

namespace Marketplace.Integration.Tests
{
    public abstract class IntegrationTestsBase : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
    {
        protected const string DefaultPassword = "P@ssw0rd1!";

        protected readonly CustomWebApplicationFactory _factory;
        protected HttpClient _client = default!;
        protected ApplicationUser TestUser { get; private set; } = default!;

        public IntegrationTestsBase(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        public async Task InitializeAsync()
        {
            await _factory.ResetDatabaseAsync();
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.Scheme);
            await EnsureTestUserExistsAsync();
        }
        public Task DisposeAsync() => Task.CompletedTask;

        private async Task EnsureTestUserExistsAsync()
        {
            TestUser = await IntegrationTestsHelper.EnsureUserExistsAsync(
                _factory.Services,
                TestAuthHandler.UserId,
                "integration@test.com",
                DefaultPassword,
                "integration-test");
        }
    }
}
