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

Dentro da rede do Docker Compose, a API acessa o banco pelo host `database`.

Os dados do PostgreSQL são mantidos no volume Docker `blogsharp_postgres_data`.

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
