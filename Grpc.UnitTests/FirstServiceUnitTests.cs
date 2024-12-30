using Basics;
using Grpc.Core;
using Grpc.FirstServer.Services;

namespace Grpc.Tests;

public class FirstServiceTests
{
    private readonly IFirstService _sut = new FirstService();

    [Fact]
    public async Task Unary_ShouldReturn_Object()
    {
        // Arrange
        var request = new Request()
        {
            Content = "Test"
        };
        var context = new TestServerCallContext(new Metadata(), CancellationToken.None);
        var expectedResponse = new Response
        {
            Message = "Hello Test from Unary with 0 headers."
        };

        // Act
        var result = await _sut.Unary(request, context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Message, result.Message);
    }
}