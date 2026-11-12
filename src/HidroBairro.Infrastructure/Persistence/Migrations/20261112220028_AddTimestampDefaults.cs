using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HidroBairro.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTimestampDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ajusta DEFAULT no schema já existente (InitialZeladoria já aplicada).
            // Não dropa tabelas nem dados.
            migrationBuilder.AlterColumn<DateTime>(
                name: "CRIADO_EM",
                table: "SOLICITACAO_SERVICO_HB",
                type: "TIMESTAMP(7)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "TIMESTAMP(7)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ATUALIZADO_EM",
                table: "SOLICITACAO_SERVICO_HB",
                type: "TIMESTAMP(7)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "TIMESTAMP(7)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CRIADO_EM",
                table: "HISTORICO_STATUS_HB",
                type: "TIMESTAMP(7)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "TIMESTAMP(7)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CRIADO_EM",
                table: "SOLICITACAO_SERVICO_HB",
                type: "TIMESTAMP(7)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TIMESTAMP(7)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ATUALIZADO_EM",
                table: "SOLICITACAO_SERVICO_HB",
                type: "TIMESTAMP(7)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TIMESTAMP(7)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CRIADO_EM",
                table: "HISTORICO_STATUS_HB",
                type: "TIMESTAMP(7)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TIMESTAMP(7)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");
        }
    }
}
