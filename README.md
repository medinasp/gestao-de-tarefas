# Gestão de Tarefas

API REST para gerenciamento de tarefas (to-do): criação, edição, remoção, listagem com filtros e busca por código. Projeto backend em .NET 8, com persistência em memória.

## Requisitos

- .NET SDK 8.0 ou superior.

Não há banco de dados a instalar: a aplicação usa o provider **InMemory** do Entity Framework Core. Os dados existem apenas enquanto a aplicação está em execução.

## Como executar

Na raiz do projeto:

```bash
dotnet run --project src/GestaoDeTarefas.Api
```

A API sobe em `http://localhost:5090`. A raiz (`/`) redireciona para o Swagger, disponível em:

```
http://localhost:5090/swagger
```

Em ambiente de desenvolvimento, a aplicação popula algumas tarefas de exemplo na primeira execução, para facilitar a exploração no Swagger. Como os dados são mantidos em memória, eles são recriados a cada reinício. Para iniciar sem dados de exemplo, execute em ambiente de produção:

```bash
ASPNETCORE_ENVIRONMENT=Production dotnet run --project src/GestaoDeTarefas.Api
```

## Como testar

```bash
dotnet test
```

São 33 testes cobrindo domínio, serviço (lógica de negócio), validações e cenários ponta a ponta da API.

## Endpoints

Base: `http://localhost:5090/api/tasks`

| Método | Rota | Descrição | Códigos de status |
|---|---|---|---|
| POST | `/api/tasks` | Cria uma tarefa | `201 Created` (+ header `Location`), `400` |
| GET | `/api/tasks` | Lista tarefas (filtros opcionais) | `200 OK`, `400` |
| GET | `/api/tasks/{code}` | Obtém uma tarefa pelo código | `200 OK`, `404` |
| PUT | `/api/tasks/{code}` | Atualiza uma tarefa | `200 OK`, `400`, `404` |
| DELETE | `/api/tasks/{code}` | Remove uma tarefa | `204 No Content`, `404` |

### Campos da tarefa

| Campo | Tipo | Obrigatório | Observação |
|---|---|---|---|
| `title` | string | sim | máximo 200 caracteres |
| `description` | string | não | máximo 1000 caracteres |
| `dueDate` | data (`YYYY-MM-DD`) | não | data de vencimento |
| `status` | enum | não | `Pending` (padrão), `InProgress`, `Done` |

O `code` (ex.: `TSK-A3FFB195`) é gerado pela API e retornado na criação.

### Filtros e busca na listagem

`GET /api/tasks` aceita `status`, `dueDate` e `search` como query string (combináveis). O `search` procura no título e na descrição (contém, sem diferenciar maiúsculas/minúsculas).

```
GET /api/tasks?status=Done
GET /api/tasks?dueDate=2026-10-01
GET /api/tasks?search=café
GET /api/tasks?status=Pending&dueDate=2026-10-01
```

### Exemplos (curl)

Criar:

```bash
curl -X POST http://localhost:5090/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"title":"Comprar café","description":"Mercado da esquina","dueDate":"2026-10-01","status":"Pending"}'
```

Resposta (`201`):

```json
{
  "code": "TSK-A3FFB195",
  "title": "Comprar café",
  "description": "Mercado da esquina",
  "dueDate": "2026-10-01",
  "status": "Pending",
  "createdAt": "2026-09-30T11:06:15.86+00:00",
  "_links": {
    "self":   { "href": "/api/tasks/TSK-A3FFB195", "method": "GET" },
    "update": { "href": "/api/tasks/TSK-A3FFB195", "method": "PUT" },
    "delete": { "href": "/api/tasks/TSK-A3FFB195", "method": "DELETE" }
  }
}
```

Listar, obter, atualizar e remover (troque `{code}` pelo código retornado):

```bash
curl http://localhost:5090/api/tasks
curl http://localhost:5090/api/tasks/{code}

curl -X PUT http://localhost:5090/api/tasks/{code} \
  -H "Content-Type: application/json" \
  -d '{"title":"Comprar café e pão","description":null,"dueDate":"2026-10-02","status":"InProgress"}'

curl -X DELETE http://localhost:5090/api/tasks/{code}
```

O `PUT` é uma substituição completa do recurso: envie todos os campos. O `status` é obrigatório no `PUT` — omiti-lo resulta em `400` (evita rebaixar o status por engano).

Erros seguem o formato **Problem Details** (RFC 7807). Uma validação de corpo inválida retorna `400` com os erros por campo; parâmetros de query inválidos (por exemplo, um `status` inexistente) também retornam `400`. Um código inexistente retorna `404`.

## Estrutura do projeto

```
src/GestaoDeTarefas.Api
  Controllers/     borda HTTP (verbos, status codes, links)
  Services/        regra de negócio
  Repositories/    acesso a dados (EF Core InMemory)
  Domain/          entidade, enum e regras de criação
  Dtos/            contratos de entrada e saída
  Mapping/         conversão domínio -> DTO
  Validation/      validações de entrada (FluentValidation)
  Infrastructure/  DbContext e tratamento de erros
tests/GestaoDeTarefas.Tests
  Domain / Services / Validation / Functional
```

## Decisões de arquitetura

- **Arquitetura em camadas** (controller → service → repository): separa responsabilidades e mantém a regra de negócio testável, sem o peso de abordagens maiores para um domínio simples.
- **RESTful com HATEOAS**: recursos, verbos e status codes corretos; cada tarefa retorna links (`_links`) para as ações disponíveis.
- **Tratamento de erros centralizado** com Problem Details (RFC 7807), em um único handler.
- **DTOs** para entrada e saída; mapeamento manual (sem biblioteca de mapeamento).
- **FluentValidation** para validação de entrada, com mensagens por campo.
- **Injeção de dependência** para desacoplar as camadas.
- **Validação do título em duas camadas**, por propósitos diferentes: a de entrada (FluentValidation) devolve `400` amigável; a do domínio (`TodoTask`) garante o invariante independentemente de quem chama a factory.
- **`code` como string** (`TSK-XXXXXXXX`): identificador de negócio legível e não sequencial, evitando enumeração de recursos.
- **`CreatedAt`** usa o relógio do sistema; `TimeProvider` não foi necessário, pois nenhuma regra depende de tempo.

Compromissos conscientes em relação ao SOLID:

- **Repositório sobre o `DbContext` concreto** (DIP): o `DbContext` já é a abstração de acesso a dados (Unit of Work + repositório) e o `ITaskRepository` isola o EF; uma interface adicional sobre o contexto seria redundante.
- **Mapeamento de exceções centralizado** (OCP): um único ponto traduz exceção em status HTTP, mantendo o conhecimento de HTTP fora do domínio; o custo é editar esse ponto ao introduzir um novo tipo de erro.
- **Links HATEOAS montados no controller** (SRP): por ser detalhe de apresentação usado apenas aqui, evita um serviço dedicado para poucas linhas.

Deixei de fora, por não agregarem a este escopo:

- **Clean Architecture / Hexagonal**: complexidade sem retorno para um CRUD.
- **Máquina de estado do status**: o status é um campo editável livremente; não há workflow exigido.
- **ETag / cache condicional**: não solicitado.

## Observações

- Projeto apenas backend, conforme o escopo.
- A porta pode ser ajustada em `src/GestaoDeTarefas.Api/Properties/launchSettings.json`.
