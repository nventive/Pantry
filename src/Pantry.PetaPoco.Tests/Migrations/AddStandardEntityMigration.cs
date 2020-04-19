using System;
using FluentMigrator;
using Pantry.Tests.StandardTestSupport;

namespace Pantry.PetaPoco.Tests.Migrations
{
    [Migration(20200419120500, "Adds the StandardEntity Db Model.")]
    public class AddStandardEntityMigration : Migration
    {
        public override void Up()
        {
            Create.Table(nameof(StandardEntity))
                .WithColumn(nameof(StandardEntity.Id)).AsString(32).NotNullable().PrimaryKey()
                .WithColumn(nameof(StandardEntity.ETag)).AsString(64).NotNullable()
                .WithColumn(nameof(StandardEntity.Timestamp)).AsDateTime().NotNullable()
                .WithColumn(nameof(StandardEntity.Name)).AsString()
                .WithColumn(nameof(StandardEntity.Age)).AsInt32()
                .WithColumn(nameof(StandardEntity.NotarizedAt)).AsDateTime()
                .WithColumn(nameof(StandardEntity.Related)).AsString(int.MaxValue)
                .WithColumn(nameof(StandardEntity.Lines)).AsString(int.MaxValue);
        }

        public override void Down()
        {
            Delete.Table(nameof(StandardEntity));
        }
    }
}
