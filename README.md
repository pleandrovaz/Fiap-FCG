# 🎮 FCG — FIAP Cloud Games API

API RESTful para gerenciamento de um catálogo de jogos digitais, com funcionalidades de biblioteca pessoal, promoções e autenticação JWT. Projeto desenvolvido como parte da pós-graduação FIAP.

---

## 📋 Índice

- [Objetivos](#-objetivos)
- [Arquitetura](#-arquitetura)
- [Tecnologias](#-tecnologias)
- [Pré-requisitos](#-pré-requisitos)
- [Configuração](#-configuração)
- [Executando a aplicação](#-executando-a-aplicação)
- [Endpoints da API](#-endpoints-da-api)
- [Autenticação](#-autenticação)
- [Modelo de Domínio](#-modelo-de-domínio)
- [Logging](#-logging)
- [Testes](#-testes)
- [Estrutura do Projeto](#-estrutura-do-projeto)

---

## 🎯 Objetivos

- Fornecer uma API para **cadastro e consulta de jogos digitais**.
- Permitir que usuários mantenham uma **biblioteca pessoal** de jogos adquiridos.
- Gerenciar **promoções** com descontos percentuais ou de valor fixo aplicados a jogos.
- Implementar **autenticação e autorização** com JWT e controle de acesso por roles (`Admin` / `User`).
- Seguir boas práticas de arquitetura em camadas, injeção de dependências e padrão `Result<T>`.

---

## 🏗 Arquitetura

O projeto segue uma **arquitetura em camadas** (Layered Architecture):

```
1-Presentation (API)        → Controllers, Middlewares, Program.cs
2-Application               → Services, DTOs, Interfaces, Resources (i18n)
3-Domain                    → Entities, Enums, Interfaces de Repositório
4-Infrastructure            → Repositórios (EF Core + Dapper), DbContext, IoC (DI)
```

**Padrões utilizados:**

| Padrão | Descrição |
|---|---|
| Repository | Abstração de acesso a dados via interfaces |
| Result\<T\> | Respostas padronizadas com `IsSuccess`, `StatusCode`, `ErrorMessage` e `Data` |
| DTO | Objetos de transferência de dados para entrada/saída da API |
| Middleware | `CorrelationIdMiddleware` para rastreabilidade de requisições |
| IoC / DI | Registro centralizado de dependências via `DependencyContainer` |

---

## 🛠 Tecnologias

| Tecnologia | Versão |
|---|---|
| .NET | 8.0 |
| C# | 12.0 |
| Entity Framework Core (Npgsql) | 8.0.16 |
| Dapper | — |
| PostgreSQL | — |
| Serilog.AspNetCore | 8.0.3 |
| Swashbuckle (Swagger) | 6.5.0 |
| AutoMapper | 12.0.1 |
| xUnit | 2.5.3 |
| Moq | 4.20.72 |

---

## ✅ Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/) (rodando na porta padrão `5432`)
- Git

---

## ⚙ Configuração

### 1. Clonar o repositório

```bash
git clone <url-do-repositorio>
cd FCG
```

### 2. Configurar o banco de dados
#Rodar com Docker
1- Baixar a imagem oficial

docker pull postgres 

2- Subir o container

docker run --name meu-postgres \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=MyApiDb \
  -p 5432:5432 \
  -d postgres

  3- Verificar se está rodando

docker ps

4- Conectar no banco
Eu uso a ferramenta abaixo, pois me traz muita solidez
DBeaver

Configuração de conexão:
Host: localhost
Porta: 5432
Database: MyApiDb
Usuário: postgres
Senha: postgres

Edite o arquivo `MyApi/src/1-Presentation/API/appsettings.json` com os dados do seu PostgreSQL:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=MyApiDb;Username=postgres;Password=SuaSenha"
  },
  "JwtSettings": {
    "Secret": "SuaChaveSecretaComNoMinimo32Caracteres!@#123456",
    "ExpirationHours": 8,
    "Issuer": "MyApi",
    "Audience": "https://localhost"
  }
}
```

> ⚠️ **Importante:** A chave `Secret` do JWT deve ter **no mínimo 32 caracteres**.

### 3. Aplicar migrations

```bash
cd MyApi/src/1-Presentation/API
dotnet ef database update
```

---

## 🚀 Executando a aplicação

```bash
cd MyApi/src/1-Presentation/API
dotnet run
```

A API estará disponível em `https://localhost:<porta>`.

### Swagger UI

Em ambiente de desenvolvimento, acesse a documentação interativa:

```
https://localhost:<porta>/swagger
```

O Swagger já está configurado com suporte a autenticação Bearer — clique em **Authorize** e insira o token JWT.

---

## 📡 Endpoints da API

### Auth (`/api/Auth`) — Público

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/Auth/register` | Registrar novo usuário |
| `POST` | `/api/Auth/login` | Autenticar e obter token JWT |
| `GET` | `/api/Auth/user/{id}` | Obter usuário por ID |
| `POST` | `/api/Auth/recuperar-senha` | Solicitar token de recuperação de senha |
| `POST` | `/api/Auth/reset-password` | Redefinir senha com token |

**Registro — Body:**
```json
{
  "name": "João Silva",
  "email": "joao@email.com",
  "password": "Senha@123"
}
```

> A senha deve ter no mínimo 8 caracteres, com letras, números e caracteres especiais.

**Login — Body:**
```json
{
  "email": "joao@email.com",
  "password": "Senha@123"
}
```

**Login — Resposta:**
```json
{
  "token": "eyJhbGciOi...",
  "usuario": {
    "id": "guid",
    "name": "João Silva",
    "email": "joao@email.com",
    "role": "User"
  },
  "expiresAt": "2025-01-01T08:00:00Z"
}
```

**Recuperar Senha — Body:**
```json
{
  "email": "joao@email.com"
}
```

**Recuperar Senha — Resposta:**
```json
{
  "message": "Token de recuperação de senha gerado com sucesso",
  "resetToken": "base64-token-gerado..."
}
```

> ⚠️ O token expira em **1 hora**. Em produção seria enviado por e-mail.

**Redefinir Senha — Body:**
```json
{
  "email": "joao@email.com",
  "token": "base64-token-recebido...",
  "newPassword": "NovaSenha@456"
}
```

**Redefinir Senha — Resposta:**
```json
{
  "message": "Senha alterada com sucesso"
}
```

---

### Jogos (`/api/Jogos`) — 🔓 Leitura pública / ✏️ Admin

| Método | Rota | Auth | Role | Descrição |
|---|---|---|---|---|
| `GET` | `/api/Jogos` | ❌ | — | Listar todos os jogos |
| `GET` | `/api/Jogos/{id}` | ❌ | — | Obter jogo por ID |
| `POST` | `/api/Jogos` | ✅ | Admin | Criar novo jogo |
| `PUT` | `/api/Jogos/{id}` | ✅ | Admin | Atualizar jogo |
| `DELETE` | `/api/Jogos/{id}` | ✅ | Admin | Excluir jogo |

**Criar/Atualizar Jogo — Body:**
```json
{
  "nome": "The Legend of Zelda",
  "descricao": "Aventura épica em mundo aberto",
  "preco": 299.90
}
```

---

### Promoções (`/api/Promocoes`) — 🔓 Leitura pública / ✏️ Admin

| Método | Rota | Auth | Role | Descrição |
|---|---|---|---|---|
| `GET` | `/api/Promocoes` | ❌ | — | Listar todas as promoções |
| `GET` | `/api/Promocoes/{id}` | ❌ | — | Obter promoção por ID |
| `POST` | `/api/Promocoes` | ✅ | Admin | Criar promoção |
| `PUT` | `/api/Promocoes/{id}` | ✅ | Admin | Atualizar promoção |
| `DELETE` | `/api/Promocoes/{id}` | ✅ | Admin | Excluir promoção |

**Criar Promoção — Body:**
```json
{
  "nome": "Black Friday",
  "idJogo": "guid-do-jogo",
  "tipoDesconto": 1,
  "valor": 30.0,
  "dataInicio": "2025-11-20T00:00:00",
  "dataFim": "2025-11-30T23:59:59"
}
```

> **TipoDesconto:** `1` = Percentual | `2` = Valor Fixo

---

### Usuários (`/api/Usuarios`) — 🔒 Autenticado

| Método | Rota | Auth | Role | Descrição |
|---|---|---|---|---|
| `GET` | `/api/Usuarios` | ✅ | Qualquer | Listar usuários |
| `GET` | `/api/Usuarios/{id}` | ✅ | Próprio / Admin | Obter usuário por ID |
| `PUT` | `/api/Usuarios/{id}` | ✅ | Próprio / Admin | Atualizar usuário |
| `DELETE` | `/api/Usuarios/{id}` | ✅ | Admin | Excluir usuário |

> Usuários comuns só podem consultar/atualizar seus próprios dados. Admins têm acesso total.

---

### Biblioteca de Jogos (`/api/BibliotecaJogos`) — 🔒 Autenticado

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| `GET` | `/api/BibliotecaJogos` | ✅ | Listar jogos da biblioteca do usuário logado |
| `GET` | `/api/BibliotecaJogos/{id}` | ✅ | Obter item da biblioteca por ID |
| `POST` | `/api/BibliotecaJogos` | ✅ | Adicionar jogo à biblioteca |
| `DELETE` | `/api/BibliotecaJogos/{id}` | ✅ | Remover jogo da biblioteca |

**Adicionar Jogo — Body:**
```json
{
  "jogoId": "guid-do-jogo",
  "promocaoId": "guid-da-promocao-ou-null"
}
```

---

## 🔐 Autenticação

A API utiliza **JWT Bearer Token** para autenticação.

### Fluxo:

1. Registre um usuário via `POST /api/Auth/register`
2. Faça login via `POST /api/Auth/login` para obter o token
3. Inclua o token no header de todas as requisições autenticadas:

```
Authorization: Bearer eyJhbGciOi...
```

### Roles:

| Role | Permissões |
|---|---|
| **User** | Consultar jogos/promoções, gerenciar sua própria biblioteca e dados |
| **Admin** | Acesso completo: CRUD de jogos, promoções, usuários |

---

## 🗃 Modelo de Domínio

```
┌──────────────┐       ┌──────────────────┐       ┌──────────────┐
│   Usuario    │       │  BibliotecaJogo  │       │     Jogo     │
├──────────────┤       ├──────────────────┤       ├──────────────┤
│ Id           │◄──────│ UsuarioId        │       │ Id           │
│ Name         │       │ JogoId           │──────►│ Nome         │
│ Email        │       │ DataCompra       │       │ Descricao    │
│ PasswordHash │       │ PrecoPago        │       │ Preco        │
│ Role         │       │ PromocaoId?      │──┐    │ Ativo        │
│ Perfil       │       └──────────────────┘  │    └──────┬───────┘
│ DataCriacao  │                              │          │
│ ResetToken?  │                              │          │
│ ResetExpira? │                              │          │
└──────────────┘
                                              │   ┌──────┴───────┐
                                              │   │   Promocao   │
                                              │   ├──────────────┤
                                              └──►│ Id           │
                                                  │ Nome         │
                                                  │ IdJogo       │
                                                  │ TipoDesconto │
                                                  │ Valor        │
                                                  │ DataInicio   │
                                                  │ DataFim      │
                                                  │ Ativa        │
                                                  └──────────────┘
```

**Enums:**

- `PerfilUsuario`: `User = 1`, `Admin = 2`
- `TipoDesconto`: `Percentual = 1`, `ValorFixo = 2`

Todas as entidades herdam de `BaseEntity` com: `Id`, `CreatedAt`, `UpdatedAt`, `IsActive`.

---

## 📝 Logging

O projeto utiliza **Serilog** com as seguintes configurações:

- **Console**: logs formatados com timestamp, nível, CorrelationId e mensagem
- **Arquivo**: logs rotativos diários em `Logs/log-YYYYMMDD.txt` (retenção de 30 dias)
- **CorrelationId**: cada requisição recebe um ID de correlação único (header `X-Correlation-Id`) para rastreabilidade
- **Request Logging**: middleware do Serilog registra automaticamente cada requisição HTTP

Exemplo de log:
```
2025-01-15 14:30:22 [INF] [abc-123-def] HTTP GET /api/Jogos responded 200 in 45ms
```

---

## 🧪 Testes

O projeto possui **97 testes** divididos em dois projetos:

### Testes Unitários (`FCG.Tests.UnitTestes`) — 85 testes

Testam a camada de serviço (Application) isoladamente com **Moq**:

- `UsuarioServiceTests` — 43 testes (incluindo recuperação de senha)
- `JogoServiceTests` — 8 testes
- `BibliotecaJogoServiceTests` — 10 testes
- `PromocaoServiceTests` — 21 testes (incluindo cenários de borda)

### Testes TDD (`FCG.Tests.TDD`) — 12 testes

Testam os controllers (Presentation) com instâncias reais e mocks de serviço:

- `PromocoesControllerTests` — 12 testes de endpoints

### Executar todos os testes

```bash
cd FCG
dotnet test
```

### Executar apenas testes unitários

```bash
dotnet test MyApi/tests/FCG.Tests/FCG.Tests.UnitTestes.csproj
```

### Executar apenas testes TDD

```bash
dotnet test MyApi/tests/FCG.Tests.TDD/FCG.Tests.TDD.csproj
```

---

## 📁 Estrutura do Projeto

```
FCG/
├── FCG.sln
└── MyApi/
    ├── src/
    │   ├── 1-Presentation/
    │   │   └── API/                        # FCG.API
    │   │       ├── Controllers/
    │   │       │   ├── AuthController.cs
    │   │       │   ├── UsuariosController.cs
    │   │       │   ├── JogosController.cs
    │   │       │   ├── PromocoesController.cs
    │   │       │   └── BibliotecaJogosController.cs
    │   │       ├── Middlewares/
    │   │       │   └── CorrelationIdMiddleware.cs
    │   │       ├── Program.cs
    │   │       └── appsettings.json
    │   ├── 2-Application/
    │   │   └── Application/                # FCG.Application
    │   │       ├── DTOs/
    │   │       ├── Interfaces/
    │   │       ├── Services/
    │   │       └── Resources/              # Mensagens i18n
    │   ├── 3-Domain/
    │   │   └── Domain/                     # FCG.Domain
    │   │       ├── Entities/
    │   │       ├── Enums/
    │   │       ├── Interfaces/Repositories/
    │   │       └── Settings/
    │   └── 4-Infrastructure/
    │       ├── Infrastructure/             # FCG.Infrastructure
    │       │   └── Data/
    │       │       ├── Context/            # AppDbContext, DapperContext
    │       │       ├── Migrations/
    │       │       └── Repositories/
    │       └── IoC/                        # IoC (DependencyContainer)
    └── tests/
        ├── FCG.Tests/                      # FCG.Tests.UnitTestes (67 testes)
        │   └── Services/
        └── FCG.Tests.TDD/                  # FCG.Tests.TDD (12 testes)
            └── Controllers/
```

---

## 📄 Licença

Este projeto foi desenvolvido para fins acadêmicos na pós-graduação FIAP.
