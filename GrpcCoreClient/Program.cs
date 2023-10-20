using Grpc.Core;
using GrpcClient;
using System.Diagnostics;
using System.Reflection;

namespace GrpcCoreClient
{
    internal class Program
    {
        private static readonly Assembly s_assembly = typeof(Program).Assembly;
        
        private static ActivitySource GrpcActivitySource { get; }
        
        static Program()
        {
            GrpcActivitySource = new(s_assembly.FullName, s_assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion);

            ActivitySource.AddActivityListener(new ActivityListener
            {
                ShouldListenTo = source => true,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) =>
                    ActivitySamplingResult.AllData,
                ActivityStopped = (activity) =>
                {
                    Console.WriteLine($"{activity?.OperationName} Stopped:\n Activity Trace ID = {activity?.TraceId}\n Activity Span ID = {activity?.SpanId}\n TraceStateString = {activity?.Context.TraceState}.\n");
                }
            });
        }
        
        static async Task Main(string[] args)
        {
            using var activity = GrpcActivitySource.StartActivity();
            activity?.SetTag("Grpc.Core Client", "1.0.0");
            activity?.SetTag("method.name", "Main");
			if (activity != null)
            {
                activity.TraceStateString = "GrpcClientTraceState";
            }
			
            // The address of the gRPC service
            string serviceAddress = "localhost:5239";

            // Create a channel to the gRPC service
            Channel channel = new Channel(serviceAddress, ChannelCredentials.Insecure);
            Console.WriteLine($"At Client: \n Activity Trace ID = {activity?.TraceId}\n Activity Span ID = {activity?.SpanId}\n TraceStateString = {activity?.TraceStateString}\n");
            var client = new Greeter.GreeterClient(channel);

            try
            {
                // Send a gRPC request with a name
                var request = new HelloRequest { Name = "Grpc Core Client" };
                var reply = client.SayHello(request);
                Console.WriteLine("From Server: \n" + reply.Message);
            }
            catch (RpcException ex)
            {
                Console.WriteLine($"RPC Error: {ex}");
            }

            // Shutdown the channel when done
            await channel.ShutdownAsync();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}