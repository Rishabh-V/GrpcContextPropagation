using Grpc.Core;
using System.Diagnostics;

namespace GrpcServiceContextPropagation.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            // Check that trace context is available even if new activity is started or otherwise as it is coming from gRPC client.
            // using var activity = Program.GrpcSource.StartActivity();
            var activity = Activity.Current;           
            var contextTraceState = activity?.Context.TraceState;
            var contextTraceId = activity?.Context.TraceId;
            var contextSpanId = activity?.Context.SpanId;
            var parentSpanId = activity?.ParentSpanId;
            Console.WriteLine($"Context Trace ID is {contextTraceId}");
            Console.WriteLine($"Context Span ID is {contextSpanId}");
            Console.WriteLine($"Context TraceStateString is {contextTraceState}");
            Console.WriteLine($"Context Parent Span ID is {parentSpanId}");
            var message = $"Hello {request.Name}!\n Context Trace ID = {contextTraceId}\n Context Span ID = {contextSpanId}\n TraceStateString = {contextTraceState}\n Context Parent Span ID = {parentSpanId} \n Server Activity Span ID = {activity?.SpanId}";
            _logger.LogInformation(message);
            return Task.FromResult(new HelloReply
            {
                Message = message
            });
        }
    }
}