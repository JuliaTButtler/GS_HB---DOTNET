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
> **Timestamps:** `CRIADO_EM` / `ATUALIZADO_EM` têm `DEFAULT CURRENT_TIMESTAMP` na `InitialZeladoria`.  
> **Seed de categoria:** fica no script SQL do grupo — a migration **não** insere categorias.

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

A `InitialZeladoria` já aplica COMMENT ON + CHECKs. Categorias: use o script SQL do grupo após o `database update`.

Pré-requisito: tabelas Java (`USUARIO_HB`, `PONTO_MONITORAMENTO_HB`, …) já criadas no mesmo schema.

## API HTTP + Swagger (etapa 5)

Contrato para Mobile (fetch/axios).

| Método | Rota | Notas |
|---|---|---|
| GET | `/api/categorias` | Categorias ativas |
| POST | `/api/solicitacoes` | Header **`Idempotency-Key`** obrigatório → **201**; replay da mesma key → **200** (não duplica) |
| GET | `/api/solicitacoes` | Query: `page`, `pageSize`, `status`, `somenteDoUsuario` |
| GET | `/api/solicitacoes/{id}` | |
| GET | `/api/solicitacoes/{id}/historico` | Trilha de status (abertura → mudanças) |
| PUT | `/api/solicitacoes/{id}` | Edita título/descrição se `RECEBIDA` |
| POST | `/api/solicitacoes/{id}/cancelar` | Query opcional `observacao` |
| PATCH | `/api/solicitacoes/{id}/status` | Body: `{ "statusNovo", "observacao?" }` |
| POST | `/api/auth/dev-token` | Emite JWT **local** (só se `Auth:LocalIssuerEnabled=true`) |

## JWT (etapa 7) — alinhado ao Java

O .NET **valida** o JWT emitido no login Java (não faz login).

### Contrato atual (Java)

| Item | Valor |
|---|---|
| Algoritmo | HS256 |
| Secret | Base64 (`Auth:Key` + `KeyIsBase64=true`) |
| `sub` | **email** do usuário (`USUARIO_HB.EMAIL`) |
| `role` | `CIDADAO` ou `GESTOR` |
| `iss` / `aud` | não usados (`ValidateIssuer/Audience=false`) |
| Validade | tipicamente ~24h (`exp` no token) |

O .NET resolve `sub` (email) → `USUARIO_HB.ID` para gravar FK nas solicitações.

### Config (`Auth`)

| Chave | Produção / Java | Development |
|---|---|---|
| `Enabled` | `true` | `true` |
| `LocalIssuerEnabled` | `false` | `true` (fallback `POST /api/auth/dev-token`) |
| `Key` | secret Base64 do Java | **mesmo** |
| `KeyIsBase64` | `true` | `true` |
| `ValidateIssuer` | `false` | `false` |
| `ValidateAudience` | `false` | `false` |

Policies: `CIDADAO`, `GESTOR`, `CidadaoOuGestor`. Sem token / inválido / email inexistente → **401**.

### Testar com token do Java

1. Login na API Java → copie o `token`
2. Swagger .NET → **Authorize** → cole só o `eyJ...` (sem `Bearer `)
3. `GET /api/categorias` → **200** se o email do `sub` existir em `USUARIO_HB`
4. Token expirado / secret errado / sem role → **401**

### Testar com JWT local (Dev)

```json
{ "email": "carla.souza@email.com", "papel": "CIDADAO" }
```

| Cenário | email | papel |
|---|---|---|
| Cidadão | email de um `CIDADAO` no seed | `CIDADAO` |
| Gestor | email de um `GESTOR` no seed | `GESTOR` |

Usuários do seed atual (`USUARIO_HB` — domínio Java; .NET só usa o id na FK):

| ID | Nome | Email | Papel | Ativo |
|---|---|---|---|---|
| 1 | Carla Souza | carla.souza@email.com | CIDADAO | S |
| 2 | Joao Mendes | joao.mendes@email.com | CIDADAO | S |
| 3 | Ana Ribeiro | ana.ribeiro@email.com | CIDADAO | S |
| 4 | Pedro Lima | pedro.lima@email.com | CIDADAO | S |
| 5 | Fernanda Dias | fernanda.dias@email.com | CIDADAO | S |
| 6 | Lucas Alves | lucas.alves@email.com | CIDADAO | S |
| 7 | Mariana Costa | mariana.costa@email.com | CIDADAO | S |
| 8 | Rafael Nunes | rafael.nunes@email.com | CIDADAO | S |
| 9 | Beatriz Rocha | beatriz.rocha@email.com | CIDADAO | S |
| 10 | Thiago Martins | thiago.martins@email.com | CIDADAO | S |
| 11 | Patricia Gomes | patricia.gomes@email.com | CIDADAO | S |
| 12 | Bruno Teixeira | bruno.teixeira@email.com | GESTOR | S |
| 13 | Camila Freitas | camila.freitas@email.com | GESTOR | S |
| 14 | Diego Araujo | diego.araujo@email.com | CIDADAO | S |
| 15 | Helena Pinto | helena.pinto@email.com | CIDADAO | N |

Para conferir no Oracle (opcional):

```sql
SELECT ID, NOME, EMAIL, PAPEL, ATIVO
FROM USUARIO_HB
ORDER BY ID;
```

Status de erro: **400** regra, **403** forbidden, **404** not found, **409** conflict, **429** rate limit.

## Rate limiting + Health (etapa 6)

### Rate limiting
- Política `api` (fixed window) aplicada em `/api/categorias` e `/api/solicitacoes`
- Partição por IP (fallback: `X-User-Id`)
- Config em `RateLimiting` (`PermitLimit`, `WindowSeconds`, `QueueLimit`)
- Excesso → **HTTP 429** + JSON `{ error, message, status }` e header `Retry-After` quando disponível
- `/`, `/health/*` e Swagger **não** entram na política `api`

Defaults: **60 req / 60s** (produção); **120 / 60s** em Development.

Para forçar 429 no teste: baixe `PermitLimit` para `5` em `appsettings.Development.json` e spamme `GET /api/categorias`.

### Health checks
| Rota | O que checa | Tags |
|---|---|---|
| `GET /health/live` | processo da API (`self`) | live |
| `GET /health/ready` | Oracle (`AddDbContextCheck`) + opcional API Java | ready |

`HealthChecks:JavaApiUrl` vazio = só Oracle no ready. Quando a Java subir, preencha a URL (ex.: `http://localhost:8080/actuator/health`) e o check `java-api` entra automaticamente.

## Rodar

Pré-requisito: .NET 8 SDK + Oracle com migration/seed aplicados.

```bash
dotnet restore HidroBairro.sln
dotnet build HidroBairro.sln
dotnet run --project src/HidroBairro.API --launch-profile http
```

- Base URL: `http://localhost:5080`
- Swagger: `http://localhost:5080/swagger`
- Smoke: `GET /` → `{ "service": "HidroBairro.API", "status": "ok" }`
- Live: `GET /health/live`
- Ready: `GET /health/ready` (precisa Oracle up)

Validação rápida no Swagger:

1. Token Java **ou** `POST /api/auth/dev-token` com `{ "email", "papel" }` → Authorize
2. `GET /api/categorias`
3. `POST /api/solicitacoes` com `Idempotency-Key: mobile-teste-1` + body (categoriaId + pontoId ou lat/long)
4. `GET /api/solicitacoes`
5. Repetir o mesmo POST/`Idempotency-Key` → mesmo recurso, sem duplicar
6. Spam de `GET /api/categorias` (com limite baixo) → **429**
7. `GET /health/live` e `GET /health/ready` → status Healthy
8. Request sem token / token inválido → **401**

```bash
dotnet test HidroBairro.sln
```

## Testes + entrega (etapa 8)

| Projeto | O que cobre |
|---|---|
| `HidroBairro.UnitTests` | Rules de domínio + `SolicitacaoService` (Moq) |
| `HidroBairro.IntegrationTests` | `WebApplicationFactory` + EF InMemory: health, JWT local, 401, categorias, create, idempotência, list |

Os integration tests **não** dependem do Oracle nem do Java: sobem a API com JWT local (`/api/auth/dev-token`) e banco em memória.

### Entrega (Portal)
1. Repo no GitHub com este README  
2. Preencher `IDENTIFICACAO_GRUPO.txt` (RM/nome de cada integrante)  
3. `.zip` com o **link do GitHub** + o `.txt` de identificação  

Quando o Java entregar Key/Issuer/Audience/claims: espelhar em `Auth` e setar `LocalIssuerEnabled=false`.

## Etapas

1. ~~Scaffold Clean Architecture~~ ✅  
2. ~~Domain (entidades + rules)~~ ✅  
3. ~~DbContext + configs Oracle (migration: você aplica)~~ ✅  
4. ~~Application (repos, UoW, services, idempotência, testes)~~ ✅  
5. ~~Endpoints Minimal API + Swagger~~ ✅  
6. ~~Rate limiting + Health Checks~~ ✅  
7. ~~JWT (validação + issuer local Dev)~~ ✅  
8. ~~Testes + README + IDENTIFICACAO_GRUPO.txt~~ ✅  
