using DiscountServiceClient;
using Grpc.Net.Client;

using var channel = GrpcChannel.ForAddress("https://localhost:7289");
var client = new DiscountRPC.DiscountRPCClient(channel);
using var cts = new CancellationTokenSource();

var reply = await client.GenerateCodesAsync(
    new GenerateCodesRequest { Count = 2000, Length = 7 }, cancellationToken: cts.Token);
Console.WriteLine("GenerateCodes: " + reply.Result);

var useCodeResponse = await client.UseCodeAsync(
    new UseCodeRequest { Code= "HMU06XG" }, cancellationToken: cts.Token);
Console.WriteLine("UseCode: " + useCodeResponse.Result);


Console.WriteLine("Press any key to exit...");
Console.ReadKey();