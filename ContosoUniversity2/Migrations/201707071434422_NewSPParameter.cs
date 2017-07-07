namespace ContosoUniversity2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewSPParameter : DbMigration
    {
        public override void Up()
        {
            Sql(@"
            CREATE PROCEDURE dbo.StudentNoByCourseIDSearch
            @Search varchar(10) = ''
            AS
            BEGIN
                SET NOCOUNT ON;

            SELECT  dbo.Course.Title AS ""CourseTitle"" , dbo.Course.CourseID, COUNT(dbo.Student.ID) AS ""StudentCount""


            FROM dbo.Student, dbo.Enrollment, dbo.Course

            WHERE dbo.Student.ID = dbo.Enrollment.StudentID AND dbo.Enrollment.CourseID = dbo.Course.CourseID AND
            (dbo.Course.Title LIKE'%' + @Search + '%' OR dbo.Course.CourseID LIKE'%' + @Search+ '%')

            GROUP BY dbo.Course.CourseID, dbo.Course.Title

            END
            ");
        }

        public override void Down()
        {
            Sql("DROP PROCEDURE dbo.StudentNoByCourseIDSearch");
        }
    }
}
