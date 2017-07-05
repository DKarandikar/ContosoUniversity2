namespace ContosoUniversity2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SingleSeminar : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Seminar", "Course_CourseID", "dbo.Course");
            DropIndex("dbo.Seminar", new[] { "Course_CourseID" });
            RenameColumn(table: "dbo.Seminar", name: "Course_CourseID", newName: "CourseID");
            AlterColumn("dbo.Seminar", "CourseID", c => c.Int(nullable: false));
            CreateIndex("dbo.Seminar", "CourseID");
            AddForeignKey("dbo.Seminar", "CourseID", "dbo.Course", "CourseID", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Seminar", "CourseID", "dbo.Course");
            DropIndex("dbo.Seminar", new[] { "CourseID" });
            AlterColumn("dbo.Seminar", "CourseID", c => c.Int());
            RenameColumn(table: "dbo.Seminar", name: "CourseID", newName: "Course_CourseID");
            CreateIndex("dbo.Seminar", "Course_CourseID");
            AddForeignKey("dbo.Seminar", "Course_CourseID", "dbo.Course", "CourseID");
        }
    }
}
