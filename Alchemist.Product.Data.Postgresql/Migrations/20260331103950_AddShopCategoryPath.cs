using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Reflection.Emit;

#nullable disable

namespace Alchemist.Product.Data.Postgresql.Migrations
{
    /// <inheritdoc />
    public partial class AddShopCategoryPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "path",
                table: "shop_category",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
                                WITH RECURSIVE shop_category_tree AS (
                                -- Базовая часть: выбираем корни
                                SELECT id, CAST('/' || id || '/' AS text) as new_path
                                FROM shop_category 
                                WHERE parent_id IS NULL
            
                                UNION ALL
            
                                -- Рекурсивная часть: идем вглубь
                                SELECT t.id, CAST(sct.new_path || t.id || '/' AS text)
                                FROM shop_category t
                                INNER JOIN shop_category_tree sct ON t.parent_id = sct.id
                            )
                            UPDATE shop_category c
                            SET path = t.new_path
                            FROM shop_category_tree t
                            WHERE c.id = t.id;");

            migrationBuilder.CreateIndex(
                            name: "ix_categories_path",
                            table: "shop_category",
                            column: "path");

            migrationBuilder.CreateIndex(
                            name: "ix_categories_parentid",
                            table: "shop_category",
                            column: "parent_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "path",
                table: "shop_category");
        }
    }
}