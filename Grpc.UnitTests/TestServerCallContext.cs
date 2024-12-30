using Grpc.Core;

namespace Grpc.Tests;

public class TestServerCallContext: ServerCallContext
{
    public TestServerCallContext(Metadata requestHeaders, CancellationToken cancellationToken = default)
    {
        RequestHeadersCore = requestHeaders;
        ResponseTrailersCore = [];
    }

    protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options)
    {
        throw new NotImplementedException();
    }

    protected override string MethodCore => "TestMethod";

    protected override string HostCore => "TestHost";

    protected override string PeerCore => "TestPeer";

    protected override DateTime DeadlineCore { get; }

    protected override Metadata RequestHeadersCore { get; }

    protected override CancellationToken CancellationTokenCore { get; }

    protected override Metadata ResponseTrailersCore { get; }

    protected override Status StatusCore { get; set; }

    protected override WriteOptions? WriteOptionsCore { get; set; }

    protected override AuthContext AuthContextCore { get; }
}