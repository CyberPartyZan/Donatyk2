using Xunit;

namespace Marketplace.Integration.Tests;

public class RabbitMqQueueIsolationTests
{
    [Fact]
    public void CustomFactory_Generates_Unique_RabbitMqPrefix_Per_Instance()
    {
        using var first = new CustomWebApplicationFactory();
        using var second = new CustomWebApplicationFactory();

        Assert.NotEqual(first.RabbitMqEndpointPrefix, second.RabbitMqEndpointPrefix);
        Assert.StartsWith("it-", first.RabbitMqEndpointPrefix);
        Assert.StartsWith("it-", second.RabbitMqEndpointPrefix);
    }

    [Fact]
    public void StripeFactory_Generates_Unique_RabbitMqPrefix_Per_Instance()
    {
        using var first = new StripeWebhookWebApplicationFactory();
        using var second = new StripeWebhookWebApplicationFactory();

        Assert.NotEqual(first.RabbitMqEndpointPrefix, second.RabbitMqEndpointPrefix);
        Assert.StartsWith("it-stripe-", first.RabbitMqEndpointPrefix);
        Assert.StartsWith("it-stripe-", second.RabbitMqEndpointPrefix);
    }
}