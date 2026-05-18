# BlogSharp

BlogSharp é uma API backend para um Blog Pessoal, desenvolvida em ASP.NET Core 8.
O projeto será evoluído em camadas, com persistência de dados, autenticação JWT e análise de qualidade com SonarQube.

## Sumário

- [Como rodar o projeto](#como-rodar-o-projeto)
- Estrutura do projeto
- Tecnologias utilizadas
- Banco de dados
- Endpoints da API
- [SonarQube](#sonarqube)
- Licença

## Como rodar o projeto

No estado atual, o projeto já possui serviços Docker para executar a API e o banco PostgreSQL em ambiente de desenvolvimento.

### Pré-requisitos

- Docker
- Docker Compose v2

### Subir a API

Na raiz do projeto, execute:

```bash
docker compose up --build
```

A API ficará disponível em:

```text
http://localhost:5000
```

O Swagger pode ser acessado em:

```text
http://localhost:5000/swagger/index.html
```

O PostgreSQL ficará disponível em:

```text
localhost:5432
```

### Parar os containers

Para encerrar o ambiente:

```bash
docker compose down
```

## Banco de dados

O projeto utiliza PostgreSQL como banco de dados relacional.

O serviço do banco é criado pelo `docker-compose.yml` com os seguintes dados de desenvolvimento:

```text
Host: localhost
Porta: 5432
Database: blogsharp
Usuário: blogsharp
Senha: blogsharp_password
```

Esses valores ficam no arquivo `.env` da raiz do projeto. O arquivo `.env.example` serve como modelo para configurar o ambiente local.

As variáveis `HOST_UID` e `HOST_GID` definem o usuário usado pelo container da API durante o desenvolvimento. Isso evita que arquivos gerados pelo `dotnet watch`, como `bin/` e `obj/`, fiquem com permissões incorretas no host.

Dentro da rede do Docker Compose, a API acessa o banco pelo host `database`.

Os dados do PostgreSQL são mantidos no volume Docker `blogsharp_postgres_data`.

## Validações nos Models

O projeto usa Data Annotations para declarar validações simples nos models, como `[Required]`, `[EmailAddress]` e `[StringLength]`. Isso deixa regras básicas visíveis na própria classe e ajuda o ASP.NET Core a validar dados recebidos pela API.

As validações de entrada também devem ser reforçadas nos DTOs, porque eles representam o contrato das requisições e respostas da API.

No CRUD de usuários, os DTOs foram separados conforme o formato necessário em cada chamada. Criamos DTOs de entrada para cadastro, atualização e login, e reutilizamos `UsuarioResponse` nas chamadas que retornam os mesmos dados públicos do usuário. Uma resposta separada só foi criada quando a saída muda da entrada, como em `UsuarioLoginResponse`, que também retorna o token JWT.

## Autenticação JWT

O token JWT do usuário guarda apenas dados úteis para identificação e autorização. Usamos o `Id` como identificador principal do usuário autenticado, o `Email` e o `Nome` como informações de contexto, e o `Tipo` como perfil de acesso para permitir regras futuras como rotas restritas a administradores.

Dados sensíveis, como senha ou hash da senha, não devem entrar no token. O objetivo é manter o JWT suficiente para validar a identidade do usuário sem transformar o token em uma cópia completa do cadastro.

## Operações Assíncronas

As interfaces de services e repositories usam `Task` e o sufixo `Async` porque essas camadas irão acessar o banco com Entity Framework Core. Acesso ao banco é uma operação de I/O: a requisição precisa aguardar a resposta do PostgreSQL, mas a thread do ASP.NET Core não precisa ficar bloqueada enquanto isso acontece.

O EF Core oferece métodos assíncronos para operações que executam I/O, como `ToListAsync`, `FirstOrDefaultAsync` e `SaveChangesAsync`. A documentação de boas práticas do ASP.NET Core também recomenda chamar APIs de acesso a dados de forma assíncrona quando elas estiverem disponíveis.

Usar `async/await` não elimina a espera da requisição e não resolve sozinho problemas de concorrência. O cuidado principal é não executar múltiplas operações paralelas no mesmo `DbContext`. Para regras de consistência, o projeto ainda deve usar constraints, índices únicos, chaves estrangeiras e transações quando necessário.

Referências:

- [Asynchronous Programming - EF Core](https://learn.microsoft.com/en-us/ef/core/miscellaneous/async)
- [ASP.NET Core Best Practices](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/best-practices)

## SonarQube

O projeto possui um serviço Docker para rodar o SonarQube localmente e analisar a qualidade do código.

Como o SonarQube não é necessário para executar a API no dia a dia, ele fica no profile `quality` do Docker Compose.

Para subir o SonarQube:

```bash
docker compose --profile quality up -d sonarqube
```

O painel ficará disponível em:

```text
http://localhost:9000
```

No primeiro acesso, use o login padrão `admin` e senha `admin`. O SonarQube irá solicitar a troca da senha.

Esta configuração usa o banco interno do SonarQube, suficiente para ambiente local de estudo e análise inicial.

As configurações básicas do projeto ficam no arquivo `sonar-project.properties`.

Caso ainda não tenha o scanner instalado:

```bash
dotnet tool install --global dotnet-sonarscanner
```

Depois de criar um token no SonarQube, a análise pode ser executada com:

```bash
dotnet sonarscanner begin /k:"blogsharp" /d:sonar.host.url="http://localhost:9000" /d:sonar.token="<SEU_TOKEN>"
dotnet build BlogSharp.sln
dotnet sonarscanner end /d:sonar.token="<SEU_TOKEN>"
```
