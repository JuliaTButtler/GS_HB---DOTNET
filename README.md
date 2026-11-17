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

### Credenciais (Oracle + JWT + health Java)

**As senhas / secret reais ficam no arquivo `IDENTIFICACAO_GRUPO.txt`** (entregue no `.zip` do Portal).  
Os `appsettings*.json` no GitHub usam **placeholders** de propósito.

Copie do `.txt` para `src/HidroBairro.API/appsettings.Development.json`:

| Campo no JSON | De onde vem no `.txt` |
|---|---|
| `ConnectionStrings:OracleConnection` | User Id + Password + Data Source |
| `Auth:Key` | JWT secret Base64 (mesmo do Java) — **substitua** o placeholder `AAA...=` |
| `Auth:KeyIsBase64` | `true` |
| `HealthChecks:JavaApiUrl` | URL de health da API Java |

```json
"ConnectionStrings": {
  "OracleConnection": "User Id=SEU_RM;Password=SUA_SENHA;Data Source=oracle.fiap.com.br:1521/ORCL;"
},
"Auth": {
  "Key": "COLE_AQUI_O_SECRET_BASE64_DO_TXT",
  "KeyIsBase64": true,
  "LocalIssuerEnabled": true
},
"HealthChecks": {
  "JavaApiUrl": "http://localhost:8080/actuator/health"
}
```

O `Key` dummy `AAA...=` no repo só existe para o projeto compilar; **sem o secret real do `.txt`, token Java e dev-token de correção não batem.**

**Não commite senha Oracle no GitHub público.**

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

### Health checks (API + Oracle + Java)
| Rota | Checks |
|---|---|
| `GET /health/live` | `self` (processo) |
| `GET /health/ready` | `oracle` (EF/DbContext) + `java-api` (HTTP na URL configurada) |

- Preencha `HealthChecks:JavaApiUrl` com o endpoint de health da API Java (ex.: `/actuator/health` ou a rota que o grupo Java expuser). Valor também no `IDENTIFICACAO_GRUPO.txt`.
- Se a URL estiver vazia ou a Java estiver offline, o check `java-api` fica **Degraded** (ainda aparece no JSON; Oracle Healthy mantém a API utilizável).
- Peça a URL exata à pessoa do Java e cole no `.txt` + `appsettings.Development.json` antes da correção/apresentação.

## Como o professor / avaliador sobe a API

Pré-requisitos: **.NET 8 SDK**, acesso ao **Oracle FIAP** do grupo, valores do **`IDENTIFICACAO_GRUPO.txt`**.

### 1) Colar credenciais
Abra `IDENTIFICACAO_GRUPO.txt` e copie para `src/HidroBairro.API/appsettings.Development.json`:
1. `OracleConnection`
2. `Auth:Key` (secret Base64) — mantenha `KeyIsBase64: true` e `LocalIssuerEnabled: true`
3. `HealthChecks:JavaApiUrl` (se a Java estiver acessível)

### 2) Subir
Na raiz do repositório:

```powershell
dotnet restore HidroBairro.sln
dotnet build HidroBairro.sln
dotnet run --project src/HidroBairro.API --launch-profile http
```

- Base: `http://localhost:5080`
- Swagger: `http://localhost:5080/swagger`
- Live: `http://localhost:5080/health/live`
- Ready: `http://localhost:5080/health/ready`

### 3) Autenticar no Swagger (duas opções)

**A — Token local (recomendado na correção; não depende do Java)**  
1. `POST /api/auth/dev-token`  
   ```json
   { "email": "EMAIL_EXISTENTE_EM_USUARIO_HB", "papel": "CIDADAO" }
   ```
   (ex.: um email do seed / tabela `USUARIO_HB`)  
2. Copiar `accessToken` → **Authorize** → colar **só** o `eyJ...` (sem a palavra Bearer)  
3. `GET /api/categorias` → 200  

**B — Token Java**  
1. Login na API Java → copiar o `token`  
2. Authorize no Swagger .NET da mesma forma  
3. Token Java costuma durar ~24h; se expirar, gere outro no Java **ou** use a opção A  

### 4) Fluxo rápido para validar a nota

1. Sem Authorize → `GET /api/categorias` → **401**  
2. Com token → `GET /api/categorias` → **200**  
3. `POST /api/solicitacoes` + header `Idempotency-Key` → **201**; repetir a mesma key → **200** (não duplica)  
4. Spam de requests (ou `PermitLimit` baixo) → **429**  
5. `GET /health/live` e `GET /health/ready` → checks `self` / `oracle` / `java-api`

```powershell
dotnet test HidroBairro.sln
```

## Testes + entrega (etapa 8)

| Projeto | O que cobre |
|---|---|
| `HidroBairro.UnitTests` | Rules de domínio + `SolicitacaoService` (Moq) |
| `HidroBairro.IntegrationTests` | `WebApplicationFactory` + EF InMemory: health, JWT local, 401, categorias, create, idempotência, list |

Os integration tests **não** dependem do Oracle nem do Java.

### Entrega (Portal)
1. Repositório no GitHub com este README  
2. `IDENTIFICACAO_GRUPO.txt` preenchido (integrantes + Oracle + JWT secret + JavaApiUrl)  
3. `.zip` = **link do GitHub** + o **`.txt`**  


## Etapas

1. ~~Scaffold Clean Architecture~~ ✅  
2. ~~Domain (entidades + rules)~~ ✅  
3. ~~DbContext + configs Oracle (migration: você aplica)~~ ✅  
4. ~~Application (repos, UoW, services, idempotência, testes)~~ ✅  
5. ~~Endpoints Minimal API + Swagger~~ ✅  
6. ~~Rate limiting + Health Checks~~ ✅  
7. ~~JWT (validação + issuer local Dev)~~ ✅  
8. ~~Testes + README + IDENTIFICACAO_GRUPO.txt~~ ✅  
