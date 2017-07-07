namespace ContosoUniversity2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewSP2 : DbMigration
    {
        public override void Up()
        {
            Sql("DROP PROCEDURE dbo.StudentNoByCourseID");

            Sql(@"
            CREATE PROCEDURE dbo.StudentNoByCourseID
            AS


            SELECT  dbo.Course.Title AS ""CourseTitle"" , dbo.Course.CourseID, COUNT(dbo.Student.ID) AS ""StudentCount""

            FROM dbo.Student, dbo.Enrollment, dbo.Course

            WHERE dbo.Student.ID = dbo.Enrollment.StudentID AND dbo.Enrollment.CourseID = dbo.Course.CourseID

            GROUP BY dbo.Course.CourseID, dbo.Course.Title

            ");
        }
        
        public override void Down()
        {
            Sql("DROP PROCEDURE dbo.StudentNoByCourseID");
            Sql(@"
            CREATE PROCEDURE dbo.StudentNoByCourseID
            AS


            SELECT  dbo.Course.Title, dbo.Course.CourseID, COUNT(dbo.Student.ID) 

            FROM dbo.Student, dbo.Enrollment, dbo.Course

            WHERE dbo.Student.ID = dbo.Enrollment.StudentID AND dbo.Enrollment.CourseID = dbo.Course.CourseID

            GROUP BY dbo.Course.CourseID, dbo.Course.Title

            ");
        }
    }
}
