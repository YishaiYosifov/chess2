using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace AnarchyChess.Api.Infrastructure.OpenAPI;

public class MethodNameOperationIdProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        var operation = context.OperationDescription.Operation;
        var methodName = context.MethodInfo.Name;
        operation.OperationId = methodName;

        return true;
    }
}
