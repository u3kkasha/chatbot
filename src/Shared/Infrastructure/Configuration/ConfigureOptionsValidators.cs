using Microsoft.Extensions.Options;

namespace Chatbot.Shared.Infrastructure.Configuration;

[OptionsValidator]
public partial class ConnectionStringsOptionsValidator : IValidateOptions<ConnectionStringsOptions>
{
    // Source-generated validator
}

[OptionsValidator]
public partial class QdrantOptionsValidator : IValidateOptions<QdrantOptions>
{
    // Source-generated validator
}

[OptionsValidator]
public partial class AiOptionsValidator : IValidateOptions<AiOptions>
{
    // Source-generated validator
}

[OptionsValidator]
public partial class ProcessingOptionsValidator : IValidateOptions<ProcessingOptions>
{
    // Source-generated validator
}
