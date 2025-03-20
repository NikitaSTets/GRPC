using DiscountServiceClient;
using Grpc.Net.Client;

using var channel = GrpcChannel.ForAddress("https://localhost:7289");
var client = new DiscountRPC.DiscountRPCClient(channel);
using var cts = new CancellationTokenSource();


var generateTask1 = client.GenerateCodesAsync(
    new GenerateCodesRequest { Count = 2000, Length = 7 }, cancellationToken: cts.Token);
var generateTask2 = client.GenerateCodesAsync(
    new GenerateCodesRequest { Count = 2000, Length = 7 }, cancellationToken: cts.Token);
var generateTask3 = client.GenerateCodesAsync(
    new GenerateCodesRequest { Count = 2000, Length = 7 }, cancellationToken: cts.Token);
var generateTask4 = client.GenerateCodesAsync(
    new GenerateCodesRequest { Count = 2000, Length = 7 }, cancellationToken: cts.Token);

var useCodeResponse = await client.UseCodeAsync(
    new UseCodeRequest { Code= "KA9R136" }, cancellationToken: cts.Token);
Console.WriteLine("UseCode: " + useCodeResponse.Result);

await Task.Delay(10000);
await generateTask1;
await generateTask2;
await generateTask3;
await generateTask4;

Console.WriteLine("Press any key to exit...");
Console.ReadKey();