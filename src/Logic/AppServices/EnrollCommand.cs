using CSharpFunctionalExtensions;
using Logic.Students;
using Logic.Utils;
using System;

namespace Logic.AppServices
{
    public sealed class EnrollCommand : ICommand
    {
        public long Id { get; set; }
        public string Course { get; set; }
        public string Grade { get; set; }

        public EnrollCommand(long id, string course, string grade)
        {
            Id = id;
            Course = course;
            Grade = grade;
        }

        private sealed class EnrollCommandHandler : ICommandHandler<EnrollCommand>
        {
            private readonly SessionFactory _sessionFactory;

            public EnrollCommandHandler(SessionFactory sessionFactory)
            {
                _sessionFactory = sessionFactory;
            }

            public Result Handle(EnrollCommand command)
            {
                var unitOfWork = new UnitOfWork(_sessionFactory);
                var courseRepository = new CourseRepository(unitOfWork);
                var studentRepository = new StudentRepository(unitOfWork);

                var student = studentRepository.GetById(command.Id);
                if (student == null)
                {
                    return Result.Fail($"No student found with id '{command.Id}'");
                }

                var course = courseRepository.GetByName(command.Course);
                if (course == null)
                {
                    return Result.Fail($"Course not found '{command.Course}'");
                }

                var success = Enum.TryParse(command.Grade, out Grade grade);
                if (!success)
                {
                    return Result.Fail($"Grade is incorrect '{command.Grade}'");
                }

                student.Enroll(course, grade);
                unitOfWork.Commit();

                return Result.Ok();
            }
        }
    }
}
