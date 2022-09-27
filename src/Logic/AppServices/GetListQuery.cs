using CSharpFunctionalExtensions;
using Dapper;
using Logic.Dtos;
using Logic.Students;
using Logic.Utils;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Logic.AppServices
{
    public sealed class GetListQuery : IQuery<List<StudentDto>>
    {
        public string EnrolledIn { get; }
        public int? NumberOfCourses { get; }

        public GetListQuery(string enrolledIn, int? numberOFCourses)
        {
            EnrolledIn = enrolledIn;
            NumberOfCourses = numberOFCourses;
        }

        private sealed class GetListQueryHandler : IQueryHandler<GetListQuery, List<StudentDto>>
        {
            private readonly ConnectionString _connectionString;

            public GetListQueryHandler(ConnectionString connectionString)
            {
                _connectionString = connectionString;
            }

            public List<StudentDto> Handle(GetListQuery query)
            {
                string sql = @"
                    SELECT s.*, e.Grade, c.Name, c.CourseName, c.Credits
                    From dbo.Students s 
                    LEFT JOIN (
                        SELECT e.StudentId, COUNT(*) Number 
                        FROM dbo.Enrollment e 
                        GROUP BY e.StudentId) t ON s.StudentId = t.StudentId 
                    LEFT JOIN dbo.Enrollment e ON e.StudentId = s.StudentId
                    LEFT JOIN dbo.Course c ON e.CourseId = c.CourseId 
                    WHERE (c.Name = @Course OR @Course IS NULL) 
                        AND (ISNULL(t.Number, 0) = @Number or @Number IS NULL)
                    ORDER BY s.StudentId ASC";
                using (SqlConnection connection = new SqlConnection(_connectionString.Value))
                {
                    var students = connection
                        .Query<StudentInDB>(sql, new
                        {
                            Course = query.EnrolledIn,
                            Number = query.NumberOfCourses
                        }).ToList();

                    var ids = students
                        .GroupBy(x => x.StudentID)
                        .Select(x => x.Key)
                        .ToList();

                    var result = new List<StudentDto>();

                    foreach (var id in ids)
                    {
                        var data = students
                            .Where(x => x.StudentID == id)
                            .ToList();

                        var dto = new StudentDto
                        {
                            Id = data[0].StudentID,
                            Name = data[0].Name,
                            Email = data[0].Email,
                            Course1 = data[0].CourseName,
                            Course1Credits = data[0].Credits,
                            Course1Grade = data[0]?.Grade.ToString()
                        };

                        if(data.Count > 1)
                        {
                            dto.Course2 = data[1].CourseName;
                            dto.Course2Credits = data[1].Credits;
                            dto.Course2Grade = data[1]?.Grade.ToString();
                        }

                        result.Add(dto);
                    }
                    return result;  
                }
            }

            private class StudentInDB
            {
                public readonly long StudentID;
                public readonly string Name;
                public readonly string Email;
                public readonly Grade? Grade;
                public readonly string CourseName;
                public readonly int? Credits;

                public StudentInDB(long studentId, string name, string email, 
                    Grade? grade, string courseName, int? credits)
                {
                    StudentID = studentId;
                    Name = name;
                    Email = email;
                    Grade = grade;
                    CourseName = courseName;
                    Credits = credits;
                }
            }
        }
    }
}
