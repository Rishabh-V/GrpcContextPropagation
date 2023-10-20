# Grpc Context Propagation

A simple ASP.NET Core gRPC service and two gRPC clients (using Grpc.NET.Client and Grpc.Core) to check if trace context is propagated from client to the server.

## Findings:
* Grpc.Net.Client based client passes the Trace Context from client to server.
* No additional code or OTel dependencies are required.
* OTel has a concept of Baggage. This is not transferred by the Grpc.Net.Client out of the box. If an application uses Baggage (though it is discouraged to do so) then they can use the OpenTelemetry.GrpcNetClient.Instrumentation library in their application to propagate the context that also includes Baggage.

## Grpc.Net Client logs:
```
// See that Trace ID is same across the logs at client and server and TraceStateString is received in server as set in the client. 
At Client:
 Activity Trace ID = 66ff184fa11000f85cee6b1f810df364
 Activity Span ID = a94548e325ab7241
 TraceStateString = GrpcClientTraceState

System.Net.Http.HttpRequestOut Stopped:
 Activity Trace ID = 66ff184fa11000f85cee6b1f810df364
 Activity Span ID = 3ef9280c612ab476
 TraceStateString = GrpcClientTraceState.

Grpc.Net.Client.GrpcOut Stopped:
 Activity Trace ID = 66ff184fa11000f85cee6b1f810df364
 Activity Span ID = 133482859f87e7f4
 TraceStateString = GrpcClientTraceState.

From Server:
Hello Grpc .NET Client!
 Context Trace ID = 66ff184fa11000f85cee6b1f810df364
 Context Span ID = 6f461696c1474769
 TraceStateString = GrpcClientTraceState
 Context Parent Span ID = 3ef9280c612ab476 (This should be same as HttpRequestOut Span ID)
 Server Activity Span ID = 6f461696c1474769
```
* Grpc.Core based client *does not* pass the Trace Context from client to the server.
* However, the applications using Grpc.Core client can use [OpenTelemetry.Instrumentation.GrpcCore](https://www.nuget.org/packages/OpenTelemetry.Instrumentation.GrpcCore/1.0.0-beta.5)) for auto-instrumentation and context propagation

## Grpc.Core Client logs:
```
// See that Trace ID is different and TraceStateString is lost in the server and not the same as set in the client.
At Client:
 Activity Trace ID = d74a0a1709abc789c8b4c0abd9c1d04a
 Activity Span ID = 0e9171fd857e09b1
 TraceStateString = GrpcClientTraceState

From Server:
Hello Grpc Core Client!
 Context Trace ID = c99c14ed6e881dcb9fa55a924d1d36d8
 Context Span ID = 8dcb57164bfd123e
 TraceStateString =
 Context Parent Span ID = 0000000000000000
 Server Activity Span ID = 8dcb57164bfd123e
```
