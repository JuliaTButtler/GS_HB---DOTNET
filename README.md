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

A Global Solution 2026/2 pede uma **CitizenTech**: tecnologia centrada no cidadão, com benefício concreto e alinhada a ODS. O piloto do grupo é a **Zona Sul de São Paulo** (Campo Limpo, Capão Redondo e M’Boi Mirim).

No 1º trimestre de 2026 a Zona Sul liderou ocorrências de alagamento/inundação na cidade (ex.: **201** registros, à frente da Zona Leste), com Campo Limpo entre os principais vetores (*Folha de S.Paulo*). A Prefeitura já monitora via **CGE**, mas o morador comum costuma depender de WhatsApp do condomínio ou “olhar pela janela” — pouco antecipação de risco e **nenhum canal simples** para pedir desobstrução de bueiro/boca de lobo e acompanhar o chamado.

**Persona:** Carla Souza, 34 anos, Capão Redondo — usa ônibus até o Terminal Campo Limpo; já perdeu tempo em rua alagada; precisa saber se consegue sair de casa **e** reportar problema de zeladoria no caminho.

ODS: **11** (cidades sustentáveis), **13** (ação climática); complementarmente 9 e 10.

### Solução proposta (produto HidroBairro Sul)

**HidroBairro Sul** é um app cidadão (React Native) + dois backends com domínios distintos e **um Oracle compartilhado**:

| Capacidade do produto | Quem entrega |
|---|---|
| Risco de alagamento em pontos monitorados (sensores + alertas) | API **Java** |
| Orientação com IA (“posso sair de casa?”, “o que fazer agora?”) | API **Java** (Spring AI / RAG local) |
| Login/cadastro e papéis `CIDADAO` / `GESTOR` | API **Java** (emite JWT) |
| Abrir e acompanhar solicitações de zeladoria | API **.NET** (este repositório) |
| Visão do gestor: alertas + status dos chamados | Java (alertas) + .NET (chamados) |
| Experiência na tela (jornada unificada) | App **Mobile** |

**Por que dois backends:** Java responde *“quem é você?”* e *“está perigoso agora?”*; .NET responde *“quero que alguém resolva isso”* e *“em que pé está meu chamado?”*. O mobile orquestra a UX; Oracle é a fonte da verdade (Mongo entra na disciplina de banco como espelho/documental).

**Jornada típica (chuva à tarde):** alerta no ponto piloto → pergunta à IA → no caminho vê boca de lobo entupida → abre solicitação no .NET → acompanha `RECEBIDA` → `EM_VISTORIA` → `CONCLUIDA`; o gestor prioriza pela área.

### Papel desta API (.NET) no produto

Este repositório é a fatia **Advanced Business Development with .NET**: backend de **zeladoria / serviço ao cidadão**, consumido pelo Mobile:

- categorias e CRUD de solicitações (criar, listar, detalhe, editar, cancelar);
- histórico e mudança de status (gestor operacional / cidadão cancela);
- **idempotência** (`Idempotency-Key`) para não duplicar envio;
- **JWT** emitido pelo Java e **validado** aqui (mesmo secret);
- **rate limiting** (HTTP 429), **Health Checks** (self + Oracle + API Java) e testes xUnit.

Deploy desta API já está disponível no render: https://hidrobairro-api-dotnet.onrender.com

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
| `HealthChecks:JavaApiUrl` | URL de health da API Java (https://hidrobairro-java-gs2.onrender.com/actuator/health)| 

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
  "JavaApiUrl": "https://hidrobairro-java-gs2.onrender.com/actuator/health"
}
```


### Banco Oracle do zero

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

### Rodar a API (local)

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

Body (exemplos reais de `USUARIO_HB`):

```json
{ "email": "carla.souza@email.com", "papel": "CIDADAO" }
```

```json
{ "email": "bruno.teixeira@email.com", "papel": "GESTOR" }
```

```json
{ "email": "george@email.com", "papel": "CIDADAO" }
```

O email precisa existir em `USUARIO_HB` (tabela abaixo). O `papel` do body deve bater com o papel cadastrado.

### Usuários de teste (`USUARIO_HB`)

Para o professor gerar `dev-token` ou validar JWT Java sem inventar e-mail:

| Id | Nome | Email | Papel | Ativo |
|---|---|---|---|---|
| 1 | Carla Souza | `carla.souza@email.com` | CIDADAO | S |
| 2 | Joao Mendes | `joao.mendes@email.com` | CIDADAO | S |
| 3 | Ana Ribeiro | `ana.ribeiro@email.com` | CIDADAO | S |
| 4 | Pedro Lima | `pedro.lima@email.com` | CIDADAO | S |
| 5 | Fernanda Dias | `fernanda.dias@email.com` | CIDADAO | S |
| 6 | Lucas Alves | `lucas.alves@email.com` | CIDADAO | S |
| 7 | Mariana Costa | `mariana.costa@email.com` | CIDADAO | S |
| 8 | Rafael Nunes | `rafael.nunes@email.com` | CIDADAO | S |
| 9 | Beatriz Rocha | `beatriz.rocha@email.com` | CIDADAO | S |
| 10 | Thiago Martins | `thiago.martins@email.com` | CIDADAO | S |
| 11 | Patricia Gomes | `patricia.gomes@email.com` | CIDADAO | S |
| 12 | Bruno Teixeira | `bruno.teixeira@email.com` | GESTOR | S |
| 13 | Camila Freitas | `camila.freitas@email.com` | GESTOR | S |
| 14 | Diego Araujo | `diego.araujo@email.com` | CIDADAO | S |
| 15 | Helena Pinto | `helena.pinto@email.com` | CIDADAO | N |
| 41 | George Harrison | `george@email.com` | CIDADAO | S |

Sugestões rápidas: cidadão → Carla / Ana / George · gestor → Bruno / Camila · **não use Helena** (`Ativo = N`).

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
| **B — token Java** | Provar integração real (renovado a cada ~24h) |

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

### Bodies de exemplo (Swagger)

Informe `pontoId` **ou** `latitude`/`longitude`. Ajuste `categoriaId` conforme o `GET /api/categorias`.

`POST /api/solicitacoes` — bueiro (Moema / Zona Sul):

```json
{
  "categoriaId": 1,
  "titulo": "Bueiro entupido na Av. Ibirapuera",
  "descricao": "Água acumulada na sarjeta após chuva; risco de alagamento no cruzamento.",
  "pontoId": null,
  "latitude": -23.6015,
  "longitude": -46.6608
}
```

`POST /api/solicitacoes` — boca de lobo (Brooklin):

```json
{
  "categoriaId": 2,
  "titulo": "Boca de lobo obstruída na Rua Verbo Divino",
  "descricao": "Folhas e entulho bloqueando a grade; rua parcialmente intransitável.",
  "pontoId": null,
  "latitude": -23.6241,
  "longitude": -46.6987
}
```

`PUT /api/solicitacoes/{id}` — edição (só se status `RECEBIDA`):

```json
{
  "titulo": "Bueiro entupido — atualizado com foto no app",
  "descricao": "Confirmei o ponto: em frente ao número 1200, água sobe na calçada."
}
```

`PATCH /api/solicitacoes/{id}/status` — gestor inicia vistoria:

```json
{
  "statusNovo": "EM_VISTORIA",
  "observacao": "Equipe de zeladoria a caminho (turno da manhã)."
}
```

`PATCH .../status` — gestor conclui:

```json
{
  "statusNovo": "CONCLUIDA",
  "observacao": "Desobstrução concluída; teste com água ok."
}
```

Status válidos: `RECEBIDA`, `EM_VISTORIA`, `CONCLUIDA`, `CANCELADA`.

---

## 7. Guia de testes no Swagger

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

**Passo 1 — Token cidadão**

`POST /api/auth/dev-token`

```json
{ "email": "ana.ribeiro@email.com", "papel": "CIDADAO" }
```

Authorize com o `accessToken`.

**Passo 2 — Categorias**

`GET /api/categorias` → anote um `id` (ex.: Bueiro / Boca de lobo).

**Passo 3 — Criar solicitação**

`POST /api/solicitacoes`  
Header: `Idempotency-Key: demo-ana-001`  

```json
{
  "categoriaId": 1,
  "titulo": "Alagamento recorrente na Rua Domingos de Morais",
  "descricao": "Bueiro sem vazão; água sobe na faixa de pedestre em chuva moderada.",
  "latitude": -23.5928,
  "longitude": -46.6352
}
```

Esperado: **201**. Anote o `id`.

**Passo 4 — Idempotência**

Repita o mesmo POST com a **mesma** `Idempotency-Key: demo-ana-001` → **200**, mesmo `id`.

**Passo 5 — Listar / detalhe / histórico / editar**

- `GET /api/solicitacoes`
- `GET /api/solicitacoes/{id}`
- `GET /api/solicitacoes/{id}/historico`
- `PUT /api/solicitacoes/{id}` com título/descrição atualizados (só `RECEBIDA`)

**Passo 6 — Gestor**

Novo `dev-token`:

```json
{ "email": "bruno.teixeira@email.com", "papel": "GESTOR" }
```

Authorize de novo →  
`GET /api/solicitacoes?somenteDoUsuario=false` →  
`PATCH /api/solicitacoes/{id}/status` com `EM_VISTORIA` (depois `CONCLUIDA`).

**Passo 7 — Outro cidadão (403)**

Token de `joao.mendes@email.com` → `GET /api/solicitacoes/{id}` do chamado da Ana → **403**.

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
| `HidroBairro.UnitTests` | Rules de domínio + `SolicitacaoService` (create/update/cancel/status/403) |
| `HidroBairro.IntegrationTests` | JWT, 401/403, CRUD solicitação, idempotência, rate limit **429**, health live/ready |

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
2. `IDENTIFICACAO_GRUPO.txt` preenchido (integrantes + **link do GitHub** + Oracle + JWT secret + JavaApiUrl)  
3. `.zip` = o **`.txt`**

### Observações finais

- O produto CitizenTech é o **HidroBairro Sul** (risco + IA + zeladoria + app); este repo entrega só o bounded context de zeladoria em .NET.
- Clean Architecture, DI, Repository + Unit of Work, `ServiceResult` → HTTP.
- Oracle compartilhado com Java; .NET só migra as 3 tabelas de zeladoria.
- JWT: Java emite, .NET valida; `dev-token` facilita correção sem depender do login Java.
- Idempotência + Rate Limiting + Health (API, banco, serviço externo) + Swagger + xUnit atendem a rubrica da disciplina.
