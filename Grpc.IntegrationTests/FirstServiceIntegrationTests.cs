using Basics;
using Grpc.IntegrationTests;

namespace Grpc.Tests.IntegrationTests;

public class FirstServiceIntegrationTests: IClassFixture<TestWebAppFactory<Program>>
{
    private readonly TestWebAppFactory<Program> _factory;

    public FirstServiceIntegrationTests(TestWebAppFactory<Program> factory)
    {
        _factory = factory;
    }


    [Fact]
    public void Unary_ShouldReturn_Message()
    {
        // Arrange
        var client = _factory.CreateGrpcClient();
        var expectedResponse = new Response
        {
            Message = "Hello Test from Unary with 2 headers."
        };

        // Act
        var actualResponse = client.Unary(new Request() { Content = "Test" });

        // Assert
        Assert.NotNull(actualResponse);
        Assert.Equal(expectedResponse.Message, actualResponse.Message);
    }
}