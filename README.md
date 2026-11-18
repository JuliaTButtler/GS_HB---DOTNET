# HidroBairro Sul — API .NET (Zeladoria)

API RESTful em ASP.NET Core (.NET 8) Minimal APIs para **solicitações de zeladoria** do cidadão — abrir, acompanhar e atualizar chamados (bueiro, boca de lobo, rua alagada) na Zona Sul de São Paulo, no produto **HidroBairro Sul** (CitizenTech · Global Solution 2026/2 · FIAP).

Repositório: https://github.com/JuliaTButtler/GS_HB---DOTNET.git

## Integrantes

| Nome | RM | Turma |
|---|---|---|
| Arthur dos Santos Cabral | 566515 | 2TDSA |
| Bruno Martins Bettio | 564939 | 2TDSA |
| José Diogo da Silva Neves | 562341 | 2TDSA |
| Júlia Tiziotto Buttler | 564975 | 2TDSA |
| Mariana Xavier Quispe | 566357 | 2TDSA |

> Credenciais de execução (Oracle, JWT secret, JavaApiUrl): arquivo **`IDENTIFICACAO_GRUPO.txt`** (entregue no `.zip` do Portal).

## Sumário

1. [Visão geral](#1-visão-geral)
2. [Arquitetura](#2-arquitetura)
3. [Instalação e execução](#3-instalação-e-execução)
4. [Autenticação JWT](#4-autenticação-jwt)
5. [Idempotência e Rate Limiting](#5-idempotência-e-rate-limiting)
6. [Documentação dos endpoints](#6-documentação-dos-endpoints)
7. [Guia de testes no Swagger](#7-guia-de-testes-no-swagger)
8. [Testes automatizados](#8-testes-automatizados)
9. [Observabilidade (Health Checks)](#9-observabilidade-health-checks)
10. [Entrega](#10-entrega)

---

## 1. Visão geral

### Contexto e problema

A Zona Sul de São Paulo concentra ocorrências relevantes de alagamento. O cidadão precisa de um canal simples para **registrar problemas de zeladoria** (bueiro entupido, boca de lobo, rua intransitável) e acompanhar o status, sem depender só de WhatsApp ou canais genéricos.

No HidroBairro Sul, risco/alertas/IA ficam na **API Java**; o **.NET** responde pela jornada “quero que alguém resolva isso” e “em que pé está meu chamado?”.

### Solução proposta

Backend .NET de **zeladoria** consumido pelo app Mobile (React Native):

- categorias de chamado;
- abertura de solicitação com **idempotência** (não duplica se o usuário apertar enviar duas vezes);
- listagem, detalhe, histórico de status, edição, cancelamento e mudança de status (gestor);
- **JWT** emitido pelo Java e **validado** nesta API (mesmo secret Base64);
- **rate limiting**, **Health Checks** (self + Oracle + API Java) e testes xUnit.

### O que a API permite

| Capacidade | Detalhe |
|---|---|
| Categorias | Listar categorias ativas de zeladoria |
| Solicitações | CRUD operacional (criar, listar, buscar, editar, cancelar) |
| Status | `RECEBIDA` → `EM_VISTORIA` → `CONCLUIDA` (ou `CANCELADA`) |
| Histórico | Trilha de mudanças de status |
| Papéis | `CIDADAO` (próprios chamados) e `GESTOR` (fila / status operacional) |
| Idempotência | Header `Idempotency-Key` no POST |
| Auth | JWT Bearer (Java) + `dev-token` local em Development |
| Proteção | Rate limit → HTTP 429 |
| Saúde | `/health/live` e `/health/ready` |

### Escopo técnico da entrega

| Área | O que foi entregue |
|---|---|
| Arquitetura | Clean Architecture (Domain / Application / Infrastructure / API) |
| API REST | Minimal APIs, validações, códigos HTTP, Swagger/OpenAPI |
| Auth | JWT Bearer (valida token Java; policies CIDADAO/GESTOR) |
| Persistência | EF Core 8 + Oracle; migrations só das 3 tabelas .NET |
| Idempotência | `Idempotency-Key` + índice único |
| Rate limiting | ASP.NET Core RateLimiter → 429 |
| Observabilidade | Health Checks: self, oracle, java-api |
| Testes | xUnit unitários (rules + services) + integração (WebApplicationFactory) |
| Documentação | Este README + Swagger + `IDENTIFICACAO_GRUPO.txt` |

### Fora do escopo desta API (.NET)

Login/cadastro, alertas, mapa de risco, IA/RAG e ingestão IoT → **API Java**.  
App mobile → disciplina de Mobile (consome Java + .NET).

### Tecnologias

- ASP.NET Core **Minimal APIs** (.NET 8) e C#
- Clean Architecture (Domain, Application, Infrastructure, API)
- Entity Framework Core 8 + Oracle.EntityFrameworkCore
- Swagger / OpenAPI (Swashbuckle)
- JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- ASP.NET Core Rate Limiting
- Microsoft.Extensions.Diagnostics.HealthChecks (+ EF Core check)
- xUnit, Moq, WebApplicationFactory, EF InMemory (testes)

---

## 2. Arquitetura

### Camadas

| Camada | Projeto | Responsabilidade |
|---|---|---|
| Domain | `HidroBairro.Domain` | Entidades, rules, exceptions |
| Application | `HidroBairro.Application` | Services, DTOs, `ServiceResult`, abstrações (repos / UoW) |
| Infrastructure | `HidroBairro.Infrastructure` | EF Core, Oracle, repositórios, migrations |
| API | `HidroBairro.API` | Minimal endpoints, Swagger, JWT, rate limit, health |
| Testes | `UnitTests` / `IntegrationTests` | Rules/services + smoke HTTP |

Dependências fluem para dentro: `API → Application/Infrastructure`; `Infrastructure → Application → Domain`. O Domain não referencia outras camadas.

### Domínio e tabelas Oracle

| Entidade | Tabela | Dono |
|---|---|---|
| `CategoriaSolicitacao` | `CATEGORIA_SOLICITACAO_HB` | .NET |
| `SolicitacaoServico` | `SOLICITACAO_SERVICO_HB` | .NET |
| `HistoricoStatus` | `HISTORICO_STATUS_HB` | .NET |
| `UsuarioRef` | `USUARIO_HB` | Java (só FK / leitura) |
| `PontoMonitoramentoRef` | `PONTO_MONITORAMENTO_HB` | Java (só FK / leitura) |

Migrations .NET **não** criam `USUARIO_HB` / `PONTO_MONITORAMENTO_HB` (`ExcludeFromMigrations`).

### Estrutura de pastas

```text
HidroBairro.sln
├── src/
│   ├── HidroBairro.Domain/
│   ├── HidroBairro.Application/
│   ├── HidroBairro.Infrastructure/
│   │   └── Persistence/          # DbContext, configs, repos, Migrations
│   └── HidroBairro.API/
│       ├── Auth/
│       ├── Endpoints/
│       └── Extensions/           # Swagger, JWT, rate limit, health
└── tests/
    ├── HidroBairro.UnitTests/
    └── HidroBairro.IntegrationTests/
```

---

## 3. Instalação e execução

### Pré-requisitos

- .NET 8 SDK (`global.json` no repo)
- Oracle FIAP (schema compartilhado com o Java; tabelas Java já criadas)
- Valores de conexão / JWT / health Java no arquivo **`IDENTIFICACAO_GRUPO.txt`**

### Clonar

```bash
git clone <URL_DO_REPOSITORIO>
cd "GS_HB - DOTNET"
```

### Configurar credenciais (obrigatório)

Os `appsettings*.json` no GitHub usam **placeholders** (sem senha real).

1. Abra `IDENTIFICACAO_GRUPO.txt`
2. Copie para `src/HidroBairro.API/appsettings.Development.json`:

| Campo JSON | Conteúdo |
|---|---|
| `ConnectionStrings:OracleConnection` | User Id + Password + Data Source do `.txt` |
| `Auth:Key` | JWT secret Base64 (mesmo do Java) |
| `Auth:KeyIsBase64` | `true` |
| `Auth:LocalIssuerEnabled` | `true` (permite `dev-token` na correção) |
| `HealthChecks:JavaApiUrl` | URL de health da API Java |

Exemplo:

```json
"ConnectionStrings": {
  "OracleConnection": "User Id=SEU_RM;Password=SUA_SENHA;Data Source=oracle.fiap.com.br:1521/ORCL;"
},
"Auth": {
  "Enabled": true,
  "LocalIssuerEnabled": true,
  "Key": "COLE_O_SECRET_BASE64_DO_TXT",
  "KeyIsBase64": true,
  "ValidateIssuer": false,
  "ValidateAudience": false
},
"HealthChecks": {
  "JavaApiUrl": "http://localhost:8080/actuator/health"
}
```


### Banco Oracle do zero (outro schema / professor)

A migration .NET cria só `CATEGORIA_SOLICITACAO_HB`, `SOLICITACAO_SERVICO_HB` e `HISTORICO_STATUS_HB`, com **FK** para tabelas Java. Por isso a ordem é obrigatória:

| Passo | O quê | Arquivo / comando |
|---|---|---|
| **1** | DDL das tabelas Java | `scripts/01_ddl_java_oracle.sql` (SQL Developer) |
| **2** | Migration .NET | `dotnet ef database update` (abaixo) |
| **3** | Seeds (usuários, pontos, categorias, amostras) | `scripts/02_seed_oracle.sql` |

```powershell
# Passo 2 — API parada; connection string já configurada
dotnet ef database update `
  --project src/HidroBairro.Infrastructure `
  --startup-project src/HidroBairro.API
```

**Não** rode o seed (passo 3) antes da migration: os `INSERT` em `CATEGORIA_*` / `SOLICITACAO_*` / `HISTORICO_*` precisam das tabelas .NET.

Se o schema **já** tem as tabelas Java (caso usual do grupo na FIAP), pule o passo 1 e use só migration (se faltar) + seed conforme necessário.

### Rodar a API

```powershell
dotnet restore HidroBairro.sln
dotnet build HidroBairro.sln
dotnet run --project src/HidroBairro.API --launch-profile http
```

| Recurso | URL |
|---|---|
| Base | `http://localhost:5080` |
| Swagger UI | `http://localhost:5080/swagger` |
| OpenAPI JSON | `http://localhost:5080/swagger/v1/swagger.json` |
| Health live | `http://localhost:5080/health/live` |
| Health ready | `http://localhost:5080/health/ready` |

---

## 4. Autenticação JWT

Esta API **não faz login**. O Java emite o JWT; o .NET **valida** o mesmo token (secret Base64 compartilhado).

### Contrato (alinhado ao Java)

| Item | Valor |
|---|---|
| Algoritmo | HS256 |
| Secret | Base64 (`Auth:Key` + `KeyIsBase64=true`) |
| `sub` | **email** do usuário (`USUARIO_HB.EMAIL`) |
| `role` | `CIDADAO` ou `GESTOR` |
| `iss` / `aud` | não validados |
| Validade típica (Java) | ~24h |

O .NET resolve `sub` (email) → `USUARIO_HB.ID` para FKs.

### Endpoints de auth nesta API

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| POST | `/api/auth/dev-token` | Não | Emite JWT **local** (só se `LocalIssuerEnabled=true`) no mesmo formato do Java |

Body:

```json
{ "email": "usuario@email.com", "papel": "CIDADAO" }
```

O email precisa existir em `USUARIO_HB`.

### Authorize no Swagger

1. Obtenha um token (**dev-token** ou login Java).
2. Copie **somente** o valor `eyJ...` (sem escrever `Bearer `).
3. Cadeado **Authorize** → colar → Authorize → Close.
4. `GET /api/categorias` → **200**.

Erro comum: colar `Bearer eyJ...` → vira `Bearer Bearer ...` → **401 invalid_token**.

### Duas formas de testar

| Opção | Quando usar |
|---|---|
| **A — `dev-token`** | Correção / demo sem depender do Java |
| **B — token Java** | Provar integração real (renovar se passar ~24h) |

Mutations e consultas de zeladoria exigem JWT (`CidadaoOuGestor`). Sem token → **401**.

---

## 5. Idempotência e Rate Limiting

### Idempotência (requisito de requisições repetidas)

`POST /api/solicitacoes` exige header **`Idempotency-Key`** (string única por tentativa de envio, máx. 80 chars).

| Situação | Resultado |
|---|---|
| Primeira vez com a key | **201** + cria a solicitação |
| Mesma key de novo | **200** + devolve a mesma (não duplica) |
| Sem header | **400** |

No Mobile: gerar UUID por toque em “Enviar”; em retry de rede, reenviar a **mesma** key.

### Rate limiting

- Política `api` (fixed window) em `/api/categorias` e `/api/solicitacoes`
- Excesso → **HTTP 429** + JSON de erro (+ `Retry-After` quando disponível)
- `/`, `/health/*` e Swagger fora da política
- Defaults: 60 req/60s (base); 120/60s em Development

Para forçar 429 no teste: baixe `RateLimiting:PermitLimit` para `5` e spamme `GET /api/categorias`.

---

## 6. Documentação dos endpoints

Documentação interativa: `http://localhost:5080/swagger`  
OpenAPI JSON: `http://localhost:5080/swagger/v1/swagger.json`

### Auth (local / Dev)

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| POST | `/api/auth/dev-token` | Não | Emite JWT local (`email` + `papel`) |

### Categorias

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| GET | `/api/categorias` | Sim | Lista categorias ativas |

### Solicitações

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| POST | `/api/solicitacoes` | Sim | Abre chamado (**Idempotency-Key** obrigatório) |
| GET | `/api/solicitacoes` | Sim | Lista (`page`, `pageSize`, `status`, `somenteDoUsuario`) |
| GET | `/api/solicitacoes/{id}` | Sim | Detalhe |
| GET | `/api/solicitacoes/{id}/historico` | Sim | Histórico de status |
| PUT | `/api/solicitacoes/{id}` | Sim | Edita título/descrição (se `RECEBIDA`) |
| POST | `/api/solicitacoes/{id}/cancelar` | Sim | Cancela (`observacao` query opcional) |
| PATCH | `/api/solicitacoes/{id}/status` | Sim | Muda status (gestor operacional / cidadão cancela) |

### Health

| Método | Rota | Descrição |
|---|---|---|
| GET | `/health/live` | Liveness (`self`) |
| GET | `/health/ready` | Readiness (`oracle` + `java-api`) |

### Body de criação (referência)

`POST /api/solicitacoes`:

```json
{
  "categoriaId": 1,
  "titulo": "Bueiro entupido",
  "descricao": "Água acumulada na sarjeta",
  "pontoId": null,
  "latitude": -23.65,
  "longitude": -46.75
}
```

Informe `pontoId` **ou** `latitude`/`longitude`.

`PATCH .../status`:

```json
{ "statusNovo": "EM_VISTORIA", "observacao": "Equipe a caminho" }
```

Status válidos: `RECEBIDA`, `EM_VISTORIA`, `CONCLUIDA`, `CANCELADA`.

---

## 7. Guia de testes no Swagger

### Checklist rápido (demo / professor)

1. Colar credenciais do `IDENTIFICACAO_GRUPO.txt` no `appsettings.Development.json`
2. `dotnet run --project src/HidroBairro.API --launch-profile http`
3. `dotnet test HidroBairro.sln` → verde
4. `/health/live` → Healthy; `/health/ready` → `oracle` Healthy (+ `java-api`)
5. Sem Authorize → `GET /api/categorias` → **401**
6. `dev-token` → Authorize → `GET /api/categorias` → **200**
7. `POST /api/solicitacoes` + `Idempotency-Key` → **201**; repetir key → **200**
8. (Opcional) token Java no Authorize → mesmo fluxo
9. Spam / limite baixo → **429**

### Passo a passo sugerido

**Passo 1 — Token local**

`POST /api/auth/dev-token`

```json
{ "email": "EMAIL_EXISTENTE_EM_USUARIO_HB", "papel": "CIDADAO" }
```

Authorize com o `accessToken`.

**Passo 2 — Categorias**

`GET /api/categorias` → anote um `id` de categoria.

**Passo 3 — Criar solicitação**

`POST /api/solicitacoes`  
Header: `Idempotency-Key: demo-001`  
Body: `categoriaId` + título/descrição + lat/long (ou `pontoId`).

Esperado: **201**. Anote o `id`.

**Passo 4 — Idempotência**

Repita o mesmo POST com a **mesma** `Idempotency-Key` → **200**, mesmo `id`.

**Passo 5 — Listar / detalhe / histórico**

- `GET /api/solicitacoes`
- `GET /api/solicitacoes/{id}`
- `GET /api/solicitacoes/{id}/historico`

**Passo 6 — Gestor (opcional)**

`dev-token` com `"papel": "GESTOR"` e email de gestor no seed →  
`GET /api/solicitacoes?somenteDoUsuario=false` →  
`PATCH /api/solicitacoes/{id}/status`.

### Cenários de erro

| Situação | Esperado |
|---|---|
| Endpoint protegido sem token | **401** |
| Token inválido / expirado / secret errado | **401** |
| Email do `sub` inexistente em `USUARIO_HB` | **401** |
| POST sem `Idempotency-Key` | **400** |
| Regra de negócio (ex.: editar fora de `RECEBIDA`) | **400** |
| Cidadão acessando chamado de outro | **403** |
| Rate limit excedido | **429** |

---

## 8. Testes automatizados

### Projetos

| Projeto | Cobertura |
|---|---|
| `HidroBairro.UnitTests` | Rules de domínio + `SolicitacaoService` (Moq); padrão AAA |
| `HidroBairro.IntegrationTests` | `WebApplicationFactory` + EF InMemory: JWT local, 401, categorias, create, idempotência, list, health |

```powershell
dotnet test HidroBairro.sln
```

Os integration tests **não** dependem do Oracle nem do Java em execução.

---

## 9. Observabilidade (Health Checks)

| Check | Onde | Significado |
|---|---|---|
| `self` | `/health/live` | Processo da API no ar |
| `oracle` | `/health/ready` | Consegue abrir conexão via EF/`AppDbContext` |
| `java-api` | `/health/ready` | HTTP GET em `HealthChecks:JavaApiUrl` |

- URL da Java no `.txt` + `appsettings` (ex.: Actuator `/actuator/health` ou rota equivalente do grupo).
- Sem URL ou Java offline → check `java-api` fica **Degraded** (ainda aparece no JSON; não impede uso da API se Oracle estiver Healthy).

Também: rate limiting (429) como proteção operacional.

---

## 10. Entrega

### Portal do Aluno

1. Repositório GitHub com este README  
2. `IDENTIFICACAO_GRUPO.txt` preenchido (integrantes + Oracle + JWT secret + JavaApiUrl + token opcional)  
3. `.zip` = **link do GitHub** + o **`.txt`**

### Observações finais

- Clean Architecture, DI, Repository + Unit of Work, `ServiceResult` → HTTP.
- Oracle compartilhado com Java; .NET só migra as 3 tabelas de zeladoria.
- JWT: Java emite, .NET valida; `dev-token` facilita correção.
- Idempotência + Rate Limiting + Health (API, banco, serviço externo) + Swagger + xUnit atendem a rubrica da disciplina.
- Valor CitizenTech: canal cidadão para zeladoria urbana no piloto Zona Sul (HidroBairro Sul).
