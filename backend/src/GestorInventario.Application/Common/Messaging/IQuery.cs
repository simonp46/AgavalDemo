using MediatR;

namespace GestorInventario.Application.Common.Messaging;

public interface IQuery<out TResponse> : IRequest<TResponse>;
