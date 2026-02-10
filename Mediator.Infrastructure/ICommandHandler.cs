using Mediator.Infrastructure.Command;
using MediatR;

namespace Mediator.Infrastructure;

public interface ICommandHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>;

public interface ICreateCommandHandler<TRequest, TResponse> : ICommandHandler<TRequest, TResponse>
    where TRequest : CreateCommand<TResponse>;

public interface ICreateCommandHandler<TResponse> : ICreateCommandHandler<CreateCommand<TResponse>, TResponse>;

public interface IUpdateCommandHandler<TRequest, TResponse> : ICommandHandler<TRequest, TResponse>
    where TRequest : UpdateCommand<TResponse>;

public interface IUpdateCommandHandler<TResponse> : IUpdateCommandHandler<UpdateCommand<TResponse>, TResponse>;