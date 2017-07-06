namespace ContosoUniversity2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SeminarInstructorLocation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Seminar", "InstructorID", c => c.Int());
            AddColumn("dbo.Seminar", "Location", c => c.String());
            CreateIndex("dbo.Seminar", "InstructorID");
            AddForeignKey("dbo.Seminar", "InstructorID", "dbo.Instructor", "ID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Seminar", "InstructorID", "dbo.Instructor");
            DropIndex("dbo.Seminar", new[] { "InstructorID" });
            DropColumn("dbo.Seminar", "Location");
            DropColumn("dbo.Seminar", "InstructorID");
        }
    }
}
