namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addIsActiveAdv05062025 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tb_Adv", "IsActive", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tb_Adv", "IsActive");
        }
    }
}
