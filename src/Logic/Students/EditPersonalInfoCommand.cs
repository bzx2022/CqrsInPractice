using CSharpFunctionalExtensions;

namespace Logic.Students
{
    public interface ICommand
    {

    }

    public interface IQuery<TResult>
    {

    }

    public interface ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        Result Handle(TCommand command);
    }

    public interface IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        TResult Handle(TQuery query);
    }
}