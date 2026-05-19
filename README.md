# BlogSharp

BlogSharp é uma API backend para um Blog Pessoal, desenvolvida em ASP.NET Core 8.
O projeto será evoluído em camadas, com persistência de dados, autenticação JWT e análise de qualidade com SonarQube.

## Sumário

- [Como rodar o projeto](#como-rodar-o-projeto)
- [Tecnologias utilizadas](#tecnologias-utilizadas)
- [Banco de dados](#banco-de-dados)
- [Contratos da API](#contratos-da-api)
- [Testes](#testes)
- [Seeders](#seeders)
- [SonarQube](#sonarqube)
- [Licença](#licença)

## Como rodar o projeto

No estado atual, o projeto já possui serviços Docker para executar a API e o banco PostgreSQL em ambiente de desenvolvimento.

### Pré-requisitos

- Docker
- Docker Compose v2

### Configurar variáveis de ambiente

Crie o arquivo `.env` na raiz do projeto usando `.env.example` como base:

```bash
cp .env.example .env
```

Para desenvolvimento local, mantenha os dados do banco alinhados com o `docker-compose.yml` e defina uma chave JWT com pelo menos 32 caracteres:

```env
POSTGRES_DB=blogsharp
POSTGRES_USER=blogsharp
POSTGRES_PASSWORD=blogsharp_password
JWT_SECRET_KEY=troque_por_uma_chave_segura_com_pelo_menos_32_caracteres
HOST_UID=1000
HOST_GID=1000
```

`HOST_UID` e `HOST_GID` devem representar o usuário do host que executa o Docker. Em Linux, geralmente é possível consultar com:

```bash
id -u
id -g
```

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

## Tecnologias utilizadas

- C# / .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Docker e Docker Compose
- Swagger / Swashbuckle
- JWT Bearer
- xUnit
- SonarQube

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

## Contratos da API

Os DTOs de resposta devem retornar apenas os campos necessários para o contrato do endpoint. Quando a especificação não pedir um campo, objeto aninhado ou relacionamento completo, a decisão do projeto é não incluir esse dado automaticamente.

No CRUD de postagens, o documento especifica que a postagem deve estar vinculada a um usuário e a um tema, mas não especifica que a resposta deve retornar os objetos completos de usuário e tema. Por isso, `PostagemResponse` retorna apenas `UsuarioId` e `TemaId`. Essa decisão foi tomada para evitar desperdício de código, processamento e envio desnecessário de dados.

## Autenticação JWT

O token JWT do usuário guarda apenas dados úteis para identificação e autorização. Usamos o `Id` como identificador principal do usuário autenticado, o `Email` e o `Nome` como informações de contexto, e o `Tipo` como perfil de acesso para permitir regras futuras como rotas restritas a administradores.

Dados sensíveis, como senha ou hash da senha, não devem entrar no token. O objetivo é manter o JWT suficiente para validar a identidade do usuário sem transformar o token em uma cópia completa do cadastro.

## Regras de Acesso

As rotas de cadastro, login, listagem de temas e listagem/filtro de postagens são públicas. As rotas que alteram cadastros existentes, postagens ou temas exigem autenticação.

Usuários podem atualizar e excluir o próprio cadastro. Administradores podem excluir qualquer usuário, mas não podem alterar dados pessoais de outros usuários.

Postagens só podem ser criadas e atualizadas pelo próprio dono. Administradores não podem alterar o texto de postagens de outros usuários, mas podem excluir qualquer postagem para moderação.

Temas são administrados apenas por usuários do tipo `Admin`.

As regras que precisam ler o usuário autenticado usam o helper `AuthUserExtensions`, em `Security/`. Ele não substitui o middleware de autenticação: o middleware valida o JWT e monta o usuário da requisição, enquanto o helper apenas lê o `Id` e o perfil `Admin` já presentes no token. O PDF define a estrutura base do projeto, mas não proíbe uma pasta auxiliar para organizar regras de segurança.

## Tratamento de Erros

O projeto usa `ErroResponse` para padronizar respostas de erro com o campo `mensagem`. Erros esperados de regra de negócio são lançados como exceções específicas e convertidos pelo `ExceptionMiddleware` para códigos HTTP coerentes, como `404 Not Found` e `409 Conflict`.

Erros inesperados também são capturados pelo `ExceptionMiddleware` e retornam:

```json
{
  "mensagem": "Erro interno no servidor."
}
```

## Testes

Os testes unitários ficam em `tests/BlogSharp.Api.Tests` e cobrem as regras principais do `UsuarioService`, `TemaService` e `PostagemService`, usando fakes simples para repositories e token.

Para executar:

```bash
dotnet test BlogSharp.sln
```

## Seeders

O projeto usa seeders para facilitar testes locais com dados mínimos.

O primeiro modo é o seeder fixo automático. Ele roda quando a API inicia em ambiente de desenvolvimento. A migration não chama o seeder; é o seeder que aplica migrations pendentes antes de inserir os dados iniciais.

Fluxo ao subir a API:

```text
API iniciou em Development
Seeders fixos executam
Migrations pendentes são aplicadas
Usuários fixos são criados se os emails ainda não existirem
Tema fixo é criado se ainda não existir
Postagem fixa é criada se ainda não existir
```

Usuários disponíveis para testes:

| Perfil | Email | Senha |
| --- | --- | --- |
| Admin | `admin@blogsharp.com` | `Admin@123` |
| Usuario | `usuario@blogsharp.com` | `Usuario@123` |

Tema inicial disponível para testes:

```text
Tecnologia
```

Postagem inicial disponível para testes:

```text
Primeira postagem BlogSharp
```

O segundo modo é o seeder dinâmico manual. Ele deve ser executado pela linha de comando quando for necessário criar uma massa maior de usuários, temas ou postagens para simular uso real:

```bash
docker compose exec api dotnet run -- seed usuarios 10
docker compose exec api dotnet run -- seed temas 5
docker compose exec api dotnet run -- seed postagens 20
```

O número final define quantos usuários serão criados. Os usuários aleatórios usam emails únicos no domínio `seed.blogsharp.local`, tipo `Usuario` ou `Admin`, foto como URL e senha padrão `Senha@123`.

No caso de temas, o número final define quantos temas serão criados. As descrições são geradas com base em nomes simples, como `Tecnologia`, `Programacao`, `Backend` e `Dotnet`, com um sufixo único para evitar duplicação.

No caso de postagens, o número final define quantas postagens serão criadas. As postagens são vinculadas a usuários e temas já cadastrados. Se os dados fixos ainda não existirem, o seeder manual de postagens cria antes os usuários e o tema inicial.

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

## Licença

Este projeto está licenciado conforme o arquivo `LICENSE`.
