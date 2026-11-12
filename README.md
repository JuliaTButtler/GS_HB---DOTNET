# HidroBairro Sul — API .NET (Zeladoria)

API de **solicitações de zeladoria** do produto HidroBairro Sul (CitizenTech · GS 2026/2 · FIAP).

Domínio desta API: abrir/acompanhar chamados (bueiro, boca de lobo, rua alagada, etc.).  
Auth/JWT é emitido pela API Java; risco/alertas/IA ficam no Java. Este backend **valida** o JWT e opera o CRUD de solicitações no Oracle compartilhado.

## Stack

| Item | Escolha |
|---|---|
| Runtime | .NET 8 (`net8.0`) + `global.json` |
| Host | ASP.NET Core **Minimal APIs** |
| ORM | EF Core 8 + Oracle.EntityFrameworkCore |
| Auth | JWT Bearer (validação; issuer = Java) |
| Testes | xUnit + WebApplicationFactory |

## Solution

```
HidroBairro.sln
├── src/
│   ├── HidroBairro.Domain
│   ├── HidroBairro.Application
│   ├── HidroBairro.Infrastructure
│   └── HidroBairro.API
└── tests/
    ├── HidroBairro.UnitTests
    └── HidroBairro.IntegrationTests
```

Dependências: `API → Application/Infrastructure` · `Infrastructure → Application → Domain`.

## Domain (etapa 2)

| Entidade | Tabela Oracle | Dono |
|---|---|---|
| `CategoriaSolicitacao` | `CATEGORIA_SOLICITACAO_HB` | .NET |
| `SolicitacaoServico` | `SOLICITACAO_SERVICO_HB` | .NET |
| `HistoricoStatus` | `HISTORICO_STATUS_HB` | .NET |
| `UsuarioRef` | `USUARIO_HB` | Java (só FK) |
| `PontoMonitoramentoRef` | `PONTO_MONITORAMENTO_HB` | Java (só FK) |

Rules: `SolicitacaoRules`, `StatusTransitionRules`, `IdempotencyRules`, `CategoriaRules`.  
Status: `RECEBIDA` → `EM_VISTORIA` → `CONCLUIDA` (ou `CANCELADA`).

## Persistência / Oracle (etapa 3)

`AppDbContext` + Fluent API em `src/HidroBairro.Infrastructure/Persistence/`.  
Migrations criam só as 3 tabelas .NET. `USUARIO_HB` e `PONTO_MONITORAMENTO_HB` estão com `ExcludeFromMigrations` (já existem no schema Java).

### Onde configurar o login Oracle

Edite a connection string em:

- `src/HidroBairro.API/appsettings.Development.json` (local — preferencial)
- ou `src/HidroBairro.API/appsettings.json`

```json
"ConnectionStrings": {
  "OracleConnection": "User Id=SEU_USUARIO;Password=SUA_SENHA;Data Source=localhost:1521/XEPDB1;"
}
```

| Parte | Exemplo | Significado |
|---|---|---|
| `User Id` | `HIDRO` | usuário Oracle do schema |
| `Password` | `...` | senha |
| `Data Source` | `localhost:1521/XEPDB1` | host:porta/service name (XEPDB1, ORCLPDB1, etc.) |

Alternativa TNS: `Data Source=ORCL;` (se tiver `tnsnames.ora`).

**Não commite senha real.** Troque o placeholder só na sua máquina (ou use User Secrets).

> **Oracle + CHECK:** não use `HasCheckConstraint` no Fluent — as aspas (`'S'`) quebram o `EXECUTE IMMEDIATE` do provider (ORA-06550). Os CHECKs vão na migration via `Sql()` com `''S''`.  
> **Timestamps:** `CRIADO_EM` / `ATUALIZADO_EM` têm `DEFAULT CURRENT_TIMESTAMP` (Initial + migration `AddTimestampDefaults` para bases já criadas).

### Comandos de migration (você roda)

Na raiz do repo (`F:\GS_HB - DOTNET`), com a API parada:

```powershell
# (uma vez) ferramenta EF
dotnet tool install --global dotnet-ef --version 8.0.11

# 1) Gera a migration (NÃO aplica no banco)
dotnet ef migrations add InitialZeladoria `
  --project src/HidroBairro.Infrastructure `
  --startup-project src/HidroBairro.API `
  --output-dir Persistence/Migrations

# 2) Revise o arquivo gerado em Persistence/Migrations:
#    - deve criar CATEGORIA_SOLICITACAO_HB, SOLICITACAO_SERVICO_HB, HISTORICO_STATUS_HB
#    - NÃO deve CREATE TABLE USUARIO_HB / PONTO_MONITORAMENTO_HB
#    - FKs para USUARIO_HB e PONTO_MONITORAMENTO_HB são esperadas

# 3) Aplica no Oracle (só quando a connection string estiver certa)
dotnet ef database update `
  --project src/HidroBairro.Infrastructure `
  --startup-project src/HidroBairro.API
```

Opcional no `Up()` da migration (COMMENT + seed):

```csharp
migrationBuilder.Sql("COMMENT ON TABLE CATEGORIA_SOLICITACAO_HB IS 'HidroBairro: categorias de solicitacao de zeladoria. Dominio DOTNET.'");
migrationBuilder.Sql("COMMENT ON TABLE SOLICITACAO_SERVICO_HB IS 'HidroBairro: solicitacoes de zeladoria. Dominio DOTNET. IDEMPOTENCY_KEY evita create duplicado.'");
migrationBuilder.Sql("COMMENT ON TABLE HISTORICO_STATUS_HB IS 'HidroBairro: historico de status das solicitacoes. Dominio DOTNET.'");

migrationBuilder.Sql(@"
INSERT INTO CATEGORIA_SOLICITACAO_HB (NOME, DESCRICAO, ATIVO) VALUES ('Bueiro entupido', 'Bueiro ou galeria obstruidos', 'S');
INSERT INTO CATEGORIA_SOLICITACAO_HB (NOME, DESCRICAO, ATIVO) VALUES ('Boca de lobo', 'Boca de lobo entupida ou danificada', 'S');
INSERT INTO CATEGORIA_SOLICITACAO_HB (NOME, DESCRICAO, ATIVO) VALUES ('Rua alagada', 'Via intransitavel por alagamento', 'S');
INSERT INTO CATEGORIA_SOLICITACAO_HB (NOME, DESCRICAO, ATIVO) VALUES ('Ponto critico', 'Ponto de risco recorrente no bairro', 'S');
");
```

Pré-requisito: tabelas Java (`USUARIO_HB`, `PONTO_MONITORAMENTO_HB`, …) já criadas no mesmo schema.

## Rodar

Pré-requisito: .NET 8 SDK.

```bash
dotnet restore HidroBairro.sln
dotnet build HidroBairro.sln
dotnet run --project src/HidroBairro.API --launch-profile http
```

- Base URL: `http://localhost:5080`
- Smoke: `GET /` → `{ "service": "HidroBairro.API", "status": "ok" }`

```bash
dotnet test HidroBairro.sln
```

## Etapas

1. ~~Scaffold Clean Architecture~~ ✅  
2. ~~Domain (entidades + rules)~~ ✅  
3. ~~DbContext + configs Oracle (migration: você aplica)~~ ✅  
4. Application services + idempotência  
5. Endpoints Minimal API + Swagger  
6. Rate limiting + Health Checks  
7. JWT alinhado ao Java  
8. Testes de negócio + README final / entrega  
