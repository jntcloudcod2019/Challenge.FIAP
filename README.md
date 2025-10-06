# System Challenge FIAP

Sistema de gerenciamento acadêmico desenvolvido para o desafio FIAP, com funcionalidades completas de autenticação, gestão de usuários, estudantes, turmas e matrículas.

##  Tecnologias Utilizadas

- **.NET 9.0** - Framework principal
- **ASP.NET Core** - API Web
- **Entity Framework Core** - ORM para banco de dados
- **SQL Server** - Banco de dados
- **JWT** - Autenticação e autorização
- **Serilog** - Sistema de logging
- **xUnit** - Testes unitários
- **Moq** - Framework de mocking
- **FluentAssertions** - Assertions mais legíveis

## 📋 Pré-requisitos

Antes de executar o projeto, certifique-se de ter instalado:

1. **.NET 9.0 SDK** - [Download aqui](https://dotnet.microsoft.com/download/dotnet/9.0)


### 2. Configuração do Banco de Dados
## PARA TER ACESSO AO SERVIDOR DE BANCO DE DADOS NÃO PRECISA, BAIXAR OU INSTALAR FERRAMENTAS., POIS ESTAMOS USANDO UM SERVIDOR SQL SERVER NA CLOUD

```

#### Opção B: SQL Server na Nuvem (Railway)
O projeto já está configurado para usar o Railway. A connection string está no `appsettings.json`:


##  Como Executar

### 1. Restaurar Pacotes

```bash
dotnet restore
```

### 2. Executar a Aplicação

```bash
dotnet run --project System.Challenge.FIAP/System.Challenge.FIAP.csproj
```

A aplicação estará disponível em:
- **API**: `https://localhost:7001`
- **Interface Web**: `https://localhost:7002`


##  Como Testar

### 1. Executar Testes Unitários

```bash
dotnet test System.Challenge.FIAP.Tests/
```

### 2. Executar Testes com Relatório Detalhado

```bash
dotnet test System.Challenge.FIAP.Tests/ --verbosity normal
``



### Erro de Build
- Execute `dotnet clean` e `dotnet restore`
- Verifique se o .NET 9.0 SDK está instalado
a



## 👥 Autores

- **Desenvolvedor** - [Seu Nome](https://github.com/seuusuario)

