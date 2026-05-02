using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Alchemist.Product.Data.Postgresql.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "brand",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    country_id = table.Column<short>(type: "smallint", nullable: true),
                    comment = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    added_ts = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("brand_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "component",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    group_id = table.Column<int>(type: "integer", nullable: true),
                    transcript = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    added_ts = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("component_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "component_group",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    parent_group_id = table.Column<int>(type: "integer", nullable: true),
                    added_ts = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_ts = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("component_group_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "country",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    transcript = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    added_ts = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("country_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "currency",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    code = table.Column<short>(type: "smallint", nullable: true),
                    name = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    full_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    added_ts = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("currency_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    transcript = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    brand_id = table.Column<int>(type: "integer", nullable: true),
                    articul = table.Column<string>(type: "character varying(63)", maxLength: 63, nullable: true),
                    init_shop_id = table.Column<int>(type: "integer", nullable: true),
                    added_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    product_type_id = table.Column<short>(type: "smallint", nullable: false),
                    added_ts = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_ts = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("product_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product_component",
                columns: table => new
                {
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    component_id = table.Column<int>(type: "integer", nullable: false),
                    sequal_number = table.Column<short>(type: "smallint", nullable: false),
                    added_ts = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_ts = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("product_component_pk", x => new { x.product_id, x.component_id });
                });

            migrationBuilder.CreateTable(
                name: "product_purpose",
                columns: table => new
                {
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    purpose_type_id = table.Column<short>(type: "smallint", nullable: false),
                    added_ts = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_ts = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("product_purpose_pkey", x => new { x.product_id, x.purpose_type_id });
                });

            migrationBuilder.CreateTable(
                name: "product_type",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    added_ts = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "purpose_component_group",
                columns: table => new
                {
                    component_group_id = table.Column<short>(type: "smallint", nullable: false),
                    purpose_type_id = table.Column<short>(type: "smallint", nullable: false),
                    comment = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: true),
                    added_ts = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_ts = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "purpose_type",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    added_ts = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("purpose_type_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shop",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    url = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: false),
                    caption = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    added_ts = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_ts = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("shop_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shop_category",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    shop_id = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    parent_id = table.Column<int>(type: "integer", nullable: true),
                    item_id = table.Column<int>(type: "integer", nullable: false),
                    added_ts = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_ts = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("shop_category_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shop_product",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    shop_id = table.Column<int>(type: "integer", nullable: false),
                    is_actual = table.Column<bool>(type: "boolean", nullable: true),
                    api_url = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: false),
                    item_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    item_url = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: false),
                    added_ts = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_ts = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("shop_product_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shop_product_category",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    shop_product_id = table.Column<long>(type: "bigint", nullable: false),
                    shop_category_id = table.Column<int>(type: "integer", nullable: false),
                    added_ts = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_ts = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("shop_product_category_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shop_product_price",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    shop_product_id = table.Column<long>(type: "bigint", nullable: false),
                    price = table.Column<double>(type: "double precision", nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    added_ts = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_ts = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("shop_product_price_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shop_settings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    parent_settings_id = table.Column<int>(type: "integer", nullable: true),
                    shop_id = table.Column<int>(type: "integer", nullable: false),
                    is_actual = table.Column<bool>(type: "boolean", nullable: true),
                    json_value = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<short>(type: "smallint", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    added_ts = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_ts = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("shop_settings_pk", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "shop_category_itemid_unique",
                table: "shop_category",
                columns: new[] { "shop_id", "item_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "shop_product_item_unique",
                table: "shop_product",
                columns: new[] { "shop_id", "item_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "shop_product_unique",
                table: "shop_product",
                columns: new[] { "shop_id", "product_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "shop_product_category_unique",
                table: "shop_product_category",
                columns: new[] { "shop_product_id", "shop_category_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "brand");

            migrationBuilder.DropTable(
                name: "component");

            migrationBuilder.DropTable(
                name: "component_group");

            migrationBuilder.DropTable(
                name: "country");

            migrationBuilder.DropTable(
                name: "currency");

            migrationBuilder.DropTable(
                name: "product");

            migrationBuilder.DropTable(
                name: "product_component");

            migrationBuilder.DropTable(
                name: "product_purpose");

            migrationBuilder.DropTable(
                name: "product_type");

            migrationBuilder.DropTable(
                name: "purpose_component_group");

            migrationBuilder.DropTable(
                name: "purpose_type");

            migrationBuilder.DropTable(
                name: "shop");

            migrationBuilder.DropTable(
                name: "shop_category");

            migrationBuilder.DropTable(
                name: "shop_product");

            migrationBuilder.DropTable(
                name: "shop_product_category");

            migrationBuilder.DropTable(
                name: "shop_product_price");

            migrationBuilder.DropTable(
                name: "shop_settings");
        }
    }
}
