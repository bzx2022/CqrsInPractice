using CSharpFunctionalExtensions;
using Logic.Students;
using Logic.Utils;

namespace Logic.AppServices
{
    public sealed class UnregisterCommand : ICommand
    {
        public long Id { get; }

        public UnregisterCommand(long id)
        {
            Id = id;
        }

        private sealed class UnregisterCommandHandler : ICommandHandler<UnregisterCommand>
        {
            private readonly SessionFactory _sessionFactory;

            public UnregisterCommandHandler(SessionFactory sessionFactory)
            {
                _sessionFactory = sessionFactory;
            }

            public Result Handle(UnregisterCommand command)
            {
                var unitOfWork = new UnitOfWork(_sessionFactory);

                var repository = new StudentRepository(unitOfWork);
                var student = repository.GetById(command.Id);
                if (student == null)
                {
                    return Result.Fail($"No Student Found for Id {command.Id}");
                }

                repository.Delete(student);
                unitOfWork.Commit();

                return Result.Ok();
            }
        }
    }
}
