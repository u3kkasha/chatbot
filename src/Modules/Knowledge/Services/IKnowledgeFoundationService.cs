using System.Collections.Generic;
using System.Threading.Tasks;
using Chatbot.Modules.Knowledge.Models;
using Chatbot.Modules.Knowledge.Models.Errors;
using OneOf;

namespace Chatbot.Modules.Knowledge.Services;

public interface IKnowledgeFoundationService
{
    ValueTask<OneOf<IReadOnlyList<KnowledgeChunk>, ServiceError, DependencyError>> RetrieveAsync(string query);
}
