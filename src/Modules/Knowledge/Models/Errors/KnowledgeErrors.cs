using System;

namespace Chatbot.Modules.Knowledge.Models.Errors;

public record DependencyError(string Message, Exception? InnerException = null);
public record ServiceError(string Message, Exception? InnerException = null);
