namespace ContosoUniversity2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewSQLVersion : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Seminar",
                c => new
                    {
                        SeminarID = c.Int(nullable: false, identity: true),
                        SeminarTime = c.DateTime(nullable: false),
                        SeminarLength = c.Int(nullable: false),
                        Course_CourseID = c.Int(),
                    })
                .PrimaryKey(t => t.SeminarID)
                .ForeignKey("dbo.Course", t => t.Course_CourseID)
                .Index(t => t.Course_CourseID);
            
            CreateTable(
                "dbo.SeminarStudent",
                c => new
                    {
                        Seminar_SeminarID = c.Int(nullable: false),
                        Student_ID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Seminar_SeminarID, t.Student_ID })
                .ForeignKey("dbo.Seminar", t => t.Seminar_SeminarID, cascadeDelete: true)
                .ForeignKey("dbo.Student", t => t.Student_ID, cascadeDelete: true)
                .Index(t => t.Seminar_SeminarID)
                .Index(t => t.Student_ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SeminarStudent", "Student_ID", "dbo.Student");
            DropForeignKey("dbo.SeminarStudent", "Seminar_SeminarID", "dbo.Seminar");
            DropForeignKey("dbo.Seminar", "Course_CourseID", "dbo.Course");
            DropIndex("dbo.SeminarStudent", new[] { "Student_ID" });
            DropIndex("dbo.SeminarStudent", new[] { "Seminar_SeminarID" });
            DropIndex("dbo.Seminar", new[] { "Course_CourseID" });
            DropTable("dbo.SeminarStudent");
            DropTable("dbo.Seminar");
        }
    }
}
