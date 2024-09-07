using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyMvcApp.Data.Content.Migrations
{
    /// <inheritdoc />
    public partial class SeedContentSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                SET IDENTITY_INSERT [content].[DataType] ON
                INSERT INTO [content].[DataType] ([Id], [Name], [Template], [Settings]) VALUES (1, N'Checkbox', N'Content/Boolean', N'{}')
                INSERT INTO [content].[DataType] ([Id], [Name], [Template], [Settings]) VALUES (2, N'Html', N'Content/Html', N'{"Rows":"12"}')
                INSERT INTO [content].[DataType] ([Id], [Name], [Template], [Settings]) VALUES (3, N'Script', N'Content/HtmlRaw', N'{"Rows":"10"}')
                INSERT INTO [content].[DataType] ([Id], [Name], [Template], [Settings]) VALUES (4, N'Select', N'Content/Select', N'{}')
                INSERT INTO [content].[DataType] ([Id], [Name], [Template], [Settings]) VALUES (5, N'TextLine', N'Content/String', N'{}')
                INSERT INTO [content].[DataType] ([Id], [Name], [Template], [Settings]) VALUES (6, N'TextBlock', N'Content/Text', N'{"Rows":"5"}')
                SET IDENTITY_INSERT [content].[DataType] OFF                
                """);

            migrationBuilder.Sql("""
                SET IDENTITY_INSERT [content].[DocumentType] ON
                INSERT INTO [content].[DocumentType] ([Id], [Name], [ViewName], [BaseId], [IsInstantiable]) VALUES (1, N'Page', N'ContentPage', NULL, 1)
                SET IDENTITY_INSERT [content].[DocumentType] OFF                
                """);

            migrationBuilder.Sql("""
                SET IDENTITY_INSERT [content].[PropertyType] ON
                INSERT INTO [content].[PropertyType] ([Id], [Name], [DisplayOrder], [DocumentTypeId], [DataTypeId], [Settings]) VALUES (1, N'Title', 0, 1, 5, N'{}')
                INSERT INTO [content].[PropertyType] ([Id], [Name], [DisplayOrder], [DocumentTypeId], [DataTypeId], [Settings]) VALUES (2, N'Body', 1, 1, 2, N'{"Rows":"16"}')
                SET IDENTITY_INSERT [content].[PropertyType] OFF                
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData("PropertyType", "Id", new object[] { 1, 2 }, "content");

            migrationBuilder.DeleteData("DocumentType", "Id", new object[] { 1 }, "content");

            migrationBuilder.DeleteData("DataType", "Id", new object[] { 1, 2, 3, 4, 5, 6 }, "content");
        }
    }
}
