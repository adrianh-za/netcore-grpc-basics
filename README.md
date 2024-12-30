# netcore-grpc-basics
Simple client server set of projects to test basics on gRPC in .Net

In order to run the client app and test apps, ensure the server *(Grpc.FirstServer)* is running and note the URL.  This server URL would then need to be set to the variable `baseUrl` in the client *(Grpc.Client)* `program.cs` file.

Was done as part of the [From Zero to Hero: gRPC in .Net](https://dometrain.com/course/from-zero-to-hero-grpc-in-dotnet/) course located at [Dometrain](https://dometrain.com/).

For this reason, the implementations may not be ideal as a few features were implemented to get an understanding of said feature.