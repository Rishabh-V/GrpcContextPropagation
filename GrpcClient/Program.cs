using Grpc.Net.Client;
using System.Diagnostics;
using System.Reflection;

namespace GrpcClient
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
                    // We will see trace and span ID from HttpClient and gRPC client.
                    Console.WriteLine($"{activity?.OperationName} Stopped:\n Activity Trace ID = {activity?.TraceId}\n Activity Span ID = {activity?.SpanId}\n TraceStateString = {activity?.Context.TraceState}.\n");
                }
            });
        }

        static void Main(string[] args)
        {
            using var activity = GrpcActivitySource.StartActivity();
            activity?.SetTag("Grpc.NET Client", "1.0.0");
            activity?.SetTag("method.name", "Main");
			if (activity != null)
            {
                activity.TraceStateString = "GrpcClientTraceState";
            }
			
            var channel = GrpcChannel.ForAddress("http://localhost:5239");
            Console.WriteLine($"At Client: \n Activity Trace ID = {activity?.TraceId}\n Activity Span ID = {activity?.SpanId}\n TraceStateString = {activity?.TraceStateString}\n");
            var client = new Greeter.GreeterClient(channel);
            var reply = client.SayHello(new HelloRequest { Name = "Grpc .NET Client" });
            Console.WriteLine("From Server: \n" + reply.Message); 
            Console.ReadLine();
        }
    }
}