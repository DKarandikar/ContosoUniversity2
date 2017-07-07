namespace ContosoUniversity2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewSP : DbMigration
    {
        public override void Up()
        {
            Sql(@"
            CREATE PROCEDURE dbo.StudentNoByCourseID
            AS


            SELECT  dbo.Course.Title, dbo.Course.CourseID, COUNT(dbo.Student.ID) 

            FROM dbo.Student, dbo.Enrollment, dbo.Course

            WHERE dbo.Student.ID = dbo.Enrollment.StudentID AND dbo.Enrollment.CourseID = dbo.Course.CourseID

            GROUP BY dbo.Course.CourseID, dbo.Course.Title

            ");
        }

        public override void Down()
        {
            Sql("DROP PROCEDURE dbo.StudentNoByCourseID");
        }
    }
}
