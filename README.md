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

A API sobe em `http://localhost:5090`. O Swagger abre automaticamente em:

```
http://localhost:5090/swagger
```

## Como testar

```bash
dotnet test
```

São 33 testes cobrindo domínio, serviço (lógica de negócio), validações e cenários ponta a ponta da API.

## Endpoints

Base: `http://localhost:5090/api/tasks`

| Método | Rota | Descrição | Sucesso |
|---|---|---|---|
| POST | `/api/tasks` | Cria uma tarefa | `201 Created` (+ header `Location`) |
| GET | `/api/tasks` | Lista tarefas (filtros opcionais) | `200 OK` |
| GET | `/api/tasks/{code}` | Obtém uma tarefa pelo código | `200 OK` / `404` |
| PUT | `/api/tasks/{code}` | Atualiza uma tarefa | `200 OK` / `400` / `404` |
| DELETE | `/api/tasks/{code}` | Remove uma tarefa | `204 No Content` / `404` |

### Campos da tarefa

| Campo | Tipo | Obrigatório | Observação |
|---|---|---|---|
| `title` | string | sim | máximo 200 caracteres |
| `description` | string | não | máximo 1000 caracteres |
| `dueDate` | data (`YYYY-MM-DD`) | não | data de vencimento |
| `status` | enum | não | `Pending` (padrão), `InProgress`, `Done` |

O `code` (ex.: `TSK-A3FFB195`) é gerado pela API e retornado na criação.

### Filtros da listagem

`GET /api/tasks` aceita `status` e/ou `dueDate` como query string (combináveis):

```
GET /api/tasks?status=Done
GET /api/tasks?dueDate=2026-10-01
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

Erros seguem o formato **Problem Details** (RFC 7807). Uma validação inválida retorna `400` com os erros por campo.

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

Deixei de fora, por não agregarem a este escopo:

- **Clean Architecture / Hexagonal**: complexidade sem retorno para um CRUD.
- **Máquina de estado do status**: o status é um campo editável livremente; não há workflow exigido.
- **ETag / cache condicional**: não solicitado.

## Observações

- Projeto apenas backend, conforme o escopo.
- A porta pode ser ajustada em `src/GestaoDeTarefas.Api/Properties/launchSettings.json`.
