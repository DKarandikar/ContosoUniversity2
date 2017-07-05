namespace ContosoUniversity2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SeminarAutoKey : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SeminarStudent", "Seminar_SeminarID", "dbo.Seminar");
            DropPrimaryKey("dbo.Seminar");
            AlterColumn("dbo.Seminar", "SeminarID", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.Seminar", "SeminarID");
            AddForeignKey("dbo.SeminarStudent", "Seminar_SeminarID", "dbo.Seminar", "SeminarID", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SeminarStudent", "Seminar_SeminarID", "dbo.Seminar");
            DropPrimaryKey("dbo.Seminar");
            AlterColumn("dbo.Seminar", "SeminarID", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.Seminar", "SeminarID");
            AddForeignKey("dbo.SeminarStudent", "Seminar_SeminarID", "dbo.Seminar", "SeminarID", cascadeDelete: true);
        }
    }
}
