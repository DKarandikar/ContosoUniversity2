namespace ContosoUniversity2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SeminarSearchSP : DbMigration
    {
        public override void Up()
        {
            Sql(@"
            CREATE PROCEDURE dbo.SeminarsByName
            @Search varchar(10) = ''
            AS
            BEGIN
                SET NOCOUNT ON;

            SELECT FORMAT (dbo.Seminar.SeminarTime, 'dd/MM/yyyy HH:mm', 'en-gb') AS ""DateAndTime"", 
            dbo.Course.Title,

            STUFF((SELECT '; ' + dbo.Student.FirstName + ' ' + dbo.Student.LastName

                FROM dbo.Enrollment, dbo.Student

                WHERE dbo.Enrollment.CourseID = dbo.Seminar.CourseID AND dbo.Student.ID = dbo.Enrollment.StudentID

                FOR XML PATH('')), 1, 1, '') AS ""FullNames"",

            CASE WHEN dbo.Seminar.Location IS NULL THEN 'No Location Yet' ELSE dbo.Seminar.Location END AS ""Location"" ,
            CASE WHEN dbo.Seminar.InstructorID IS NULL THEN 'No Instructor Yet' ELSE dbo.Instructor.FirstName + ' ' + dbo.Instructor.LastName END AS ""Instructor""

            FROM dbo.Seminar

            JOIN dbo.Course ON dbo.Course.CourseID = dbo.Seminar.CourseID
            LEFT JOIN dbo.Instructor ON dbo.Instructor.ID = dbo.Seminar.InstructorID
            WHERE STUFF((SELECT '; ' + dbo.Student.FirstName + ' ' + dbo.Student.LastName

                FROM dbo.Enrollment, dbo.Student

                WHERE dbo.Enrollment.CourseID = dbo.Seminar.CourseID AND dbo.Student.ID = dbo.Enrollment.StudentID

                FOR XML PATH('')),1,1,'') LIKE '%' + @Search + '%'

            ORDER BY dbo.Seminar.SeminarTime


            END
            ");
        }

        public override void Down()
        {
            Sql("DROP PROCEDURE dbo.SeminarsByName");
        }
    }
}
