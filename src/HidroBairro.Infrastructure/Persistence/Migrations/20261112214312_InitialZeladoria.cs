using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HidroBairro.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialZeladoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CATEGORIA_SOLICITACAO_HB",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NOME = table.Column<string>(type: "VARCHAR2(80)", unicode: false, maxLength: 80, nullable: false),
                    DESCRICAO = table.Column<string>(type: "VARCHAR2(255)", unicode: false, maxLength: 255, nullable: true),
                    ATIVO = table.Column<string>(type: "VARCHAR2(1)", unicode: false, maxLength: 1, nullable: false, defaultValue: "S")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CATEGORIA_SOLICITACAO_HB", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SOLICITACAO_SERVICO_HB",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    PROTOCOLO = table.Column<string>(type: "VARCHAR2(30)", unicode: false, maxLength: 30, nullable: false),
                    USUARIO_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    CATEGORIA_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    PONTO_ID = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    TITULO = table.Column<string>(type: "VARCHAR2(120)", unicode: false, maxLength: 120, nullable: false),
                    DESCRICAO = table.Column<string>(type: "VARCHAR2(1000)", unicode: false, maxLength: 1000, nullable: false),
                    LATITUDE = table.Column<decimal>(type: "DECIMAL(10,7)", precision: 10, scale: 7, nullable: true),
                    LONGITUDE = table.Column<decimal>(type: "DECIMAL(10,7)", precision: 10, scale: 7, nullable: true),
                    STATUS = table.Column<string>(type: "VARCHAR2(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "RECEBIDA"),
                    IDEMPOTENCY_KEY = table.Column<string>(type: "VARCHAR2(80)", unicode: false, maxLength: 80, nullable: true),
                    CRIADO_EM = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ATUALIZADO_EM = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SOLICITACAO_SERVICO_HB", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SOLICITACAO_HB_CATEGORIA",
                        column: x => x.CATEGORIA_ID,
                        principalTable: "CATEGORIA_SOLICITACAO_HB",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SOLICITACAO_HB_PONTO",
                        column: x => x.PONTO_ID,
                        principalTable: "PONTO_MONITORAMENTO_HB",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SOLICITACAO_HB_USUARIO",
                        column: x => x.USUARIO_ID,
                        principalTable: "USUARIO_HB",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HISTORICO_STATUS_HB",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    SOLICITACAO_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    STATUS_ANTERIOR = table.Column<string>(type: "VARCHAR2(20)", unicode: false, maxLength: 20, nullable: true),
                    STATUS_NOVO = table.Column<string>(type: "VARCHAR2(20)", unicode: false, maxLength: 20, nullable: false),
                    USUARIO_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    OBSERVACAO = table.Column<string>(type: "VARCHAR2(500)", unicode: false, maxLength: 500, nullable: true),
                    CRIADO_EM = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HISTORICO_STATUS_HB", x => x.ID);
                    table.ForeignKey(
                        name: "FK_HIST_HB_SOLICITACAO",
                        column: x => x.SOLICITACAO_ID,
                        principalTable: "SOLICITACAO_SERVICO_HB",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HIST_HB_USUARIO",
                        column: x => x.USUARIO_ID,
                        principalTable: "USUARIO_HB",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_CATEGORIA_HB_NOME",
                table: "CATEGORIA_SOLICITACAO_HB",
                column: "NOME",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HISTORICO_STATUS_HB_SOLICITACAO_ID",
                table: "HISTORICO_STATUS_HB",
                column: "SOLICITACAO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_HISTORICO_STATUS_HB_USUARIO_ID",
                table: "HISTORICO_STATUS_HB",
                column: "USUARIO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SOLICITACAO_SERVICO_HB_CATEGORIA_ID",
                table: "SOLICITACAO_SERVICO_HB",
                column: "CATEGORIA_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SOLICITACAO_SERVICO_HB_PONTO_ID",
                table: "SOLICITACAO_SERVICO_HB",
                column: "PONTO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SOLICITACAO_SERVICO_HB_USUARIO_ID",
                table: "SOLICITACAO_SERVICO_HB",
                column: "USUARIO_ID");

            // Oracle já permite vários NULL em UNIQUE — sem filtro estilo SQL Server.
            migrationBuilder.CreateIndex(
                name: "UQ_SOLICITACAO_HB_IDEMPOTENCY",
                table: "SOLICITACAO_SERVICO_HB",
                column: "IDEMPOTENCY_KEY",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_SOLICITACAO_HB_PROTOCOLO",
                table: "SOLICITACAO_SERVICO_HB",
                column: "PROTOCOLO",
                unique: true);

            // CHECKs / COMMENT via PL/SQL com aspas duplicadas (EXECUTE IMMEDIATE do Oracle EF).
            migrationBuilder.Sql("""
                BEGIN
                  EXECUTE IMMEDIATE 'ALTER TABLE CATEGORIA_SOLICITACAO_HB ADD CONSTRAINT CHK_CATEGORIA_HB_ATIVO CHECK (ATIVO IN (''S'', ''N''))';
                  EXECUTE IMMEDIATE 'ALTER TABLE SOLICITACAO_SERVICO_HB ADD CONSTRAINT CHK_SOLICITACAO_HB_STATUS CHECK (STATUS IN (''RECEBIDA'', ''EM_VISTORIA'', ''CONCLUIDA'', ''CANCELADA''))';
                  EXECUTE IMMEDIATE 'ALTER TABLE SOLICITACAO_SERVICO_HB ADD CONSTRAINT CHK_SOLICITACAO_HB_LOCAL CHECK (PONTO_ID IS NOT NULL OR (LATITUDE IS NOT NULL AND LONGITUDE IS NOT NULL))';
                  EXECUTE IMMEDIATE 'ALTER TABLE HISTORICO_STATUS_HB ADD CONSTRAINT CHK_HIST_HB_STATUS_NOVO CHECK (STATUS_NOVO IN (''RECEBIDA'', ''EM_VISTORIA'', ''CONCLUIDA'', ''CANCELADA''))';
                  EXECUTE IMMEDIATE 'ALTER TABLE HISTORICO_STATUS_HB ADD CONSTRAINT CHK_HIST_HB_STATUS_ANT CHECK (STATUS_ANTERIOR IS NULL OR STATUS_ANTERIOR IN (''RECEBIDA'', ''EM_VISTORIA'', ''CONCLUIDA'', ''CANCELADA''))';

                  EXECUTE IMMEDIATE 'COMMENT ON TABLE CATEGORIA_SOLICITACAO_HB IS ''HidroBairro: categorias de solicitacao de zeladoria. Dominio DOTNET.''';
                  EXECUTE IMMEDIATE 'COMMENT ON TABLE SOLICITACAO_SERVICO_HB IS ''HidroBairro: solicitacoes de zeladoria. Dominio DOTNET. IDEMPOTENCY_KEY evita create duplicado.''';
                  EXECUTE IMMEDIATE 'COMMENT ON TABLE HISTORICO_STATUS_HB IS ''HidroBairro: historico de status das solicitacoes. Dominio DOTNET.''';

                  EXECUTE IMMEDIATE 'INSERT INTO CATEGORIA_SOLICITACAO_HB (NOME, DESCRICAO, ATIVO) VALUES (''Bueiro entupido'', ''Bueiro ou galeria obstruidos'', ''S'')';
                  EXECUTE IMMEDIATE 'INSERT INTO CATEGORIA_SOLICITACAO_HB (NOME, DESCRICAO, ATIVO) VALUES (''Boca de lobo'', ''Boca de lobo entupida ou danificada'', ''S'')';
                  EXECUTE IMMEDIATE 'INSERT INTO CATEGORIA_SOLICITACAO_HB (NOME, DESCRICAO, ATIVO) VALUES (''Rua alagada'', ''Via intransitavel por alagamento'', ''S'')';
                  EXECUTE IMMEDIATE 'INSERT INTO CATEGORIA_SOLICITACAO_HB (NOME, DESCRICAO, ATIVO) VALUES (''Ponto critico'', ''Ponto de risco recorrente no bairro'', ''S'')';
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HISTORICO_STATUS_HB");

            migrationBuilder.DropTable(
                name: "SOLICITACAO_SERVICO_HB");

            migrationBuilder.DropTable(
                name: "CATEGORIA_SOLICITACAO_HB");
        }
    }
}
