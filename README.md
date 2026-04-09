# Fiap-FCG

Projeto de pós-graduação da FIAP.

## Estrutura do Projeto

```
Fiap-FCG/
├── src/
│   ├── Fiap.FCG.API/           # Camada de apresentação (Web API)
│   ├── Fiap.FCG.Application/   # Camada de aplicação (serviços, DTOs)
│   ├── Fiap.FCG.Domain/        # Camada de domínio (entidades, interfaces)
│   └── Fiap.FCG.Infrastructure/# Camada de infraestrutura (repositórios, banco de dados)
└── tests/
    └── Fiap.FCG.Tests/         # Testes unitários
```

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Como Executar

```bash
# Restaurar dependências
dotnet restore

# Compilar
dotnet build

# Executar a API
dotnet run --project src/Fiap.FCG.API

# Executar os testes
dotnet test
```

## Tecnologias

- ASP.NET Core Web API
- xUnit (testes)

