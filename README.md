# BlogSharp

BlogSharp é uma API backend para um Blog Pessoal, desenvolvida em [ASP.NET Core 8](https://learn.microsoft.com/aspnet/core), [PostgreSQL](https://www.postgresql.org/), [Docker](https://www.docker.com/), [JWT](https://jwt.io/), integração com IA e análise de qualidade com [SonarQube](https://www.sonarsource.com/products/sonarqube/).

Eu mantive este README focado em duas coisas: como rodar o projeto e quais decisões técnicas foram tomadas. Endpoints e contratos detalhados ficam no Swagger, porque ele reflete a API executável.

## Sumário

- [1. Pré-requisitos](#1-pré-requisitos)
- [2. Configuração do ambiente](#2-configuração-do-ambiente)
- [3. Executando a aplicação](#3-executando-a-aplicação)
- [4. Swagger e autenticação](#4-swagger-e-autenticação)
- [5. Banco de dados e migrations](#5-banco-de-dados-e-migrations)
- [6. Seeders](#6-seeders)
  - [6.1. Seed automático](#61-seed-automático)
  - [6.2. Seed manual](#62-seed-manual)
- [7. Integração com IA](#7-integração-com-ia)
- [8. Testes](#8-testes)
- [9. SonarQube](#9-sonarqube)
  - [9.1. Subir o SonarQube](#91-subir-o-sonarqube)
  - [9.2. Configurar token](#92-configurar-token)
  - [9.3. Instalar scanner](#93-instalar-scanner)
  - [9.4. Rodar análise](#94-rodar-análise)
- [10. Decisões do projeto](#10-decisões-do-projeto)
  - [10.1. DTOs e contratos mínimos](#101-dtos-e-contratos-mínimos)
  - [10.2. Data Annotations](#102-data-annotations)
  - [10.3. Operações assíncronas](#103-operações-assíncronas)
  - [10.4. Regras de acesso](#104-regras-de-acesso)
  - [10.5. Integração com IA](#105-integração-com-ia)
  - [10.6. Segredos fora do código](#106-segredos-fora-do-código)
  - [10.7. SonarQube como ferramenta de inspeção](#107-sonarqube-como-ferramenta-de-inspeção)
  - [10.8. Hotspot do Dockerfile](#108-hotspot-do-dockerfile)
- [11. Licença](#11-licença)

## 1. Pré-requisitos

- [Docker](https://docs.docker.com/get-started/)
- [Docker Compose v2](https://docs.docker.com/compose/)
- [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0), para rodar testes, migrations manuais ou SonarQube pela máquina local
- [`dotnet-sonarscanner`](https://docs.sonarsource.com/sonarqube-server/analyzing-source-code/scanners/dotnet/introduction/), apenas para executar a análise do SonarQube

## 2. Configuração do ambiente

Crie o arquivo `.env` na raiz do projeto usando o [.env.example](.env.example) como base:

```bash
cp .env.example .env
```

Configuração mínima para desenvolvimento local:

```env
POSTGRES_DB=blogsharp
POSTGRES_USER=blogsharp
POSTGRES_PASSWORD=blogsharp_password
JWT_SECRET_KEY=troque_por_uma_chave_segura_com_pelo_menos_32_caracteres

IA_ENABLED=false
IA_PROVIDER=OpenRouter
IA_BASE_URL=https://openrouter.ai
IA_API_KEY=
IA_MODEL=
IA_SITE_URL=
IA_APP_NAME=BlogSharp

HOST_UID=1000
HOST_GID=1000

SONAR_TOKEN=
```

`HOST_UID` e `HOST_GID` são usados apenas para evitar problema de permissão em arquivos gerados pelo container no Linux. Na maioria das instalações Linux o valor `1000` já funciona; se der problema de permissão em `bin/`, `obj/` ou arquivos gerados pelo Docker, confira os valores com:

```bash
id -u
id -g
```

Sempre que alterar variáveis do `.env`, recrie o container da API para que o Docker Compose injete os novos valores:

```bash
docker compose up -d --force-recreate api
```

## 3. Executando a aplicação

Na raiz do projeto, suba a API e o PostgreSQL:

```bash
docker compose up --build
```

API: [http://localhost:5000](http://localhost:5000)

PostgreSQL: `localhost:5432`

Para parar os containers:

```bash
docker compose down
```

Para parar e remover também os volumes locais:

```bash
docker compose down -v
```

Use `down -v` apenas quando quiser apagar os dados locais do PostgreSQL.

## 4. Swagger e autenticação

O Swagger pode ser acessado em [http://localhost:5000/swagger/index.html](http://localhost:5000/swagger/index.html).

Para testar rotas protegidas:

1. Faça login no endpoint `POST /api/usuarios/login`.
2. Copie o valor retornado no campo `token`.
3. Clique em `Authorize` no Swagger.
4. Cole apenas o token e confirme.

## 5. Banco de dados e migrations

O projeto usa [PostgreSQL](https://www.postgresql.org/) como banco relacional. Dentro da rede do [Docker Compose](https://docs.docker.com/compose/), a API acessa o banco pelo host `database`.

Os dados ficam no volume Docker `blogsharp_postgres_data`.

No fluxo normal com Docker, não é necessário rodar migrations manualmente. Ao iniciar em ambiente de desenvolvimento, os seeders aplicam migrations pendentes antes de inserir os dados iniciais.

Se eu quiser aplicar migrations fora desse fluxo automático, com o PostgreSQL já rodando, uso:

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet ef database update --project src/BlogSharp.Api/BlogSharp.Api.csproj
```

## 6. Seeders

Eu mantive dois modos de seed: um automático para dados mínimos de desenvolvimento e outro manual para criar massa de dados quando for necessário testar melhor a aplicação.

### 6.1. Seed automático

Quando a API inicia em ambiente `Development`, os seeders fixos executam:

- aplicam migrations pendentes;
- criam usuários fixos se os emails ainda não existirem;
- criam um tema inicial se ele ainda não existir;
- criam uma postagem inicial se ela ainda não existir.

Usuários disponíveis para testes:

| Perfil | Email | Senha |
| --- | --- | --- |
| Admin | `admin@blogsharp.com` | `Admin@123` |
| Usuario | `usuario@blogsharp.com` | `Usuario@123` |


### 6.2. Seed manual

Para criar dados aleatórios, use os comandos abaixo com a API em execução:

```bash
docker compose exec api dotnet run -- seed usuarios 10
docker compose exec api dotnet run -- seed temas 5
docker compose exec api dotnet run -- seed postagens 20
```

O número final define a quantidade de registros que será criada.

Usuários aleatórios usam emails únicos no domínio `seed.blogsharp.local` e senha padrão:

```text
Senha@123
```

As postagens aleatórias são vinculadas a usuários e temas já cadastrados. Se os dados fixos ainda não existirem, o seeder manual de postagens cria antes os usuários e o tema inicial.

## 7. Integração com IA

Quando a integração está habilitada, ao cadastrar uma postagem a API envia o conteúdo para um provedor externo de IA e salva na postagem:

- resumo;
- tags;
- categoria.

A integração fica desabilitada por padrão:

```env
IA_ENABLED=false
```

Com a IA desabilitada, o cadastro de postagens continua funcionando e os campos `ResumoIA`, `TagsIA` e `CategoriaIA` ficam vazios.

Para habilitar:

```env
IA_ENABLED=true
IA_PROVIDER=OpenRouter
IA_BASE_URL=https://openrouter.ai
IA_API_KEY=sua_chave_da_openrouter
IA_MODEL=openai/gpt-5.2
IA_SITE_URL=http://localhost:5000
IA_APP_NAME=BlogSharp
```

Depois de alterar o `.env`, recrie o container da API:

```bash
docker compose up -d --force-recreate api
```

Também existe a rota protegida `POST /api/ia/resumir`, usada para gerar resumo, categoria e tags a partir de um texto sem criar uma postagem.

## 8. Testes

Os testes ficam em [tests/BlogSharp.Api.Tests](tests/BlogSharp.Api.Tests).

Para executar todos os testes:

```bash
dotnet test BlogSharp.sln
```

Para executar apenas os testes de integração:

```bash
dotnet test BlogSharp.sln --filter FullyQualifiedName~Integration
```

Os testes unitários cobrem regras de service e provider de IA com fakes simples. Os testes de integração sobem a API em memória e validam fluxos HTTP principais.

## 9. SonarQube

O projeto possui um serviço [Docker](https://docs.docker.com/) para rodar o [SonarQube](https://www.sonarsource.com/products/sonarqube/) localmente. Ele fica no profile `quality` para não subir junto com API e banco quando eu só quero desenvolver ou testar a aplicação.

### 9.1. Subir o SonarQube

```bash
docker compose --profile quality up -d sonarqube
```

Painel: [http://localhost:9000](http://localhost:9000)

No primeiro acesso, use:

```text
Login: admin
Senha: admin
```

O SonarQube irá solicitar a troca da senha.

### 9.2. Configurar token

Crie um token no [SonarQube](https://docs.sonarsource.com/sonarqube-server/user-guide/managing-tokens/) e adicione no `.env`:

```env
SONAR_TOKEN=seu_token_do_sonarqube
```

### 9.3. Instalar scanner

A análise é executada pela máquina local, não dentro do container do SonarQube.

```bash
dotnet tool install --global dotnet-sonarscanner
```

### 9.4. Rodar análise

O projeto usa o script `scripts/sonar.sh` para evitar repetir manualmente o fluxo `begin`, `build`, `test` e `end` do scanner .NET.

```bash
./scripts/sonar.sh
```

Ao final, o relatório fica em [http://localhost:9000/dashboard?id=BlogSharp](http://localhost:9000/dashboard?id=BlogSharp).

## 10. Decisões do projeto

### 10.1. DTOs e contratos mínimos

Eu optei por respostas com apenas os campos necessários para cada endpoint. No caso de `PostagemResponse`, o PDF pede que a postagem esteja vinculada a usuário e tema, mas não exige retornar os objetos completos. Por isso, a resposta retorna `UsuarioId` e `TemaId`, sem expandir `UsuarioResponse` ou `TemaResponse`.

Essa escolha evita carregar e trafegar dados que o contrato não pediu.

### 10.2. Data Annotations

Eu usei [Data Annotations](https://learn.microsoft.com/dotnet/api/system.componentmodel.dataannotations) para validações simples, como campos obrigatórios, email e tamanho de texto. Isso deixa regras básicas visíveis nos DTOs e models sem criar uma camada extra só para validações simples.

### 10.3. Operações assíncronas

Services e repositories usam `Task` e o sufixo `Async` porque acessam o banco com [Entity Framework Core](https://learn.microsoft.com/ef/core/). Como acesso ao banco é uma operação de I/O, o `async/await` evita bloquear a thread do [ASP.NET Core](https://learn.microsoft.com/aspnet/core) enquanto o PostgreSQL responde.

### 10.4. Regras de acesso

O PDF define autenticação e controle por tipo de usuário, mas não detalha todas as regras finas. Eu defini regras explícitas para evitar comportamento ambíguo:

- cadastro público sempre cria usuário comum;
- usuário pode atualizar e excluir o próprio cadastro;
- administrador pode excluir usuários;
- alteração de privilégio fica em rota administrativa separada;
- dono pode criar e atualizar a própria postagem;
- administrador pode excluir postagens para moderação;
- temas são administrados apenas por administradores.

### 10.5. Integração com IA

O PDF sugere [OpenAI API](https://platform.openai.com/docs), [Gemini API](https://ai.google.dev/gemini-api/docs) ou [Azure AI Services](https://azure.microsoft.com/products/ai-services). Eu usei [OpenRouter](https://openrouter.ai/) com um modelo da OpenAI por uma questão prática: durante os testes, o Gemini retornou erro `403` de chave inválida mesmo com contas e chaves diferentes; a API direta da OpenAI exige créditos pagos; e o Azure ficou burocrático demais para este escopo, porque exige várias etapas de cadastro e configuração só para conseguir uma chave de API.

Com OpenRouter, o projeto mantém o objetivo do desafio: consumir uma API externa de IA, tratar resposta JSON e enriquecer postagens com resumo, tags e categoria. Também deixei `IA_ENABLED=false` por padrão para a API continuar funcionando mesmo sem chave de IA configurada.

### 10.6. Segredos fora do código

Chave JWT, token do SonarQube e chave da IA ficam no `.env` local. O repositório versiona apenas [.env.example](.env.example), sem valores sensíveis.

### 10.7. SonarQube como ferramenta de inspeção

O PDF pede SonarQube, integração com build, métricas e relatórios, mas não define meta mínima de cobertura nem exige aprovação obrigatória no Quality Gate padrão. Por isso, eu uso o SonarQube como ferramenta de inspeção: corrigir bugs, vulnerabilidades, hotspots relevantes e más práticas, sem criar testes artificiais apenas para subir porcentagem.

### 10.8. Hotspot do Dockerfile

O SonarQube aponta hotspot no [Dockerfile](Dockerfile) porque a imagem base do SDK .NET pode executar como `root` quando usada isoladamente.

No fluxo atual, considerei esse risco aceitável para ambiente local porque a API roda pelo Docker Compose com o usuário do host:

```yaml
user: "${HOST_UID:-1000}:${HOST_GID:-1000}"
```

Se o projeto ganhar um Dockerfile de produção, a imagem deve definir um usuário não-root diretamente.

## 11. Licença

Este projeto está licenciado conforme o arquivo [LICENSE](LICENSE).
