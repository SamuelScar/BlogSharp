# BlogSharp

BlogSharp é uma API backend para um Blog Pessoal, desenvolvida em ASP.NET Core 8.
O projeto será evoluído em camadas, com persistência de dados, autenticação JWT e análise de qualidade com SonarQube.

## Sumário

- [Como rodar o projeto](#como-rodar-o-projeto)
- [Tecnologias utilizadas](#tecnologias-utilizadas)
- [Banco de dados](#banco-de-dados)
- [Contratos da API](#contratos-da-api)
- [Integração com IA](#integração-com-ia)
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

Para desenvolvimento local, mantenha os dados do banco alinhados com o `docker-compose.yml`, defina uma chave JWT com pelo menos 32 caracteres e mantenha a integração com IA desabilitada até a escolha do provedor:

```env
POSTGRES_DB=blogsharp
POSTGRES_USER=blogsharp
POSTGRES_PASSWORD=blogsharp_password
JWT_SECRET_KEY=troque_por_uma_chave_segura_com_pelo_menos_32_caracteres
IA_ENABLED=false
IA_PROVIDER=
IA_BASE_URL=
IA_API_KEY=
IA_MODEL=
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

### Autenticar no Swagger

Para testar rotas protegidas, faça login em `POST /api/usuarios/login`, copie o valor do campo `token`, clique em `Authorize` no Swagger, cole apenas o token e confirme. Não é necessário escrever `Bearer` antes do token.

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

### Migrations

No fluxo normal com Docker, não é necessário rodar migrations manualmente. Ao iniciar em ambiente de desenvolvimento, os seeders aplicam migrations pendentes antes de inserir os dados iniciais.

Se for necessário aplicar migrations manualmente, mantenha o PostgreSQL rodando e execute com o `dotnet-ef` instalado:

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet ef database update --project src/BlogSharp.Api/BlogSharp.Api.csproj
```

## Validações nos Models

O projeto usa Data Annotations para declarar validações simples nos models, como `[Required]`, `[EmailAddress]` e `[StringLength]`. Isso deixa regras básicas visíveis na própria classe e ajuda o ASP.NET Core a validar dados recebidos pela API.

As validações de entrada também devem ser reforçadas nos DTOs, porque eles representam o contrato das requisições e respostas da API.

No CRUD de usuários, os DTOs foram separados conforme o formato necessário em cada chamada. Criamos DTOs de entrada para cadastro, atualização e login, e reutilizamos `UsuarioResponse` nas chamadas que retornam os mesmos dados públicos do usuário. Uma resposta separada só foi criada quando a saída muda da entrada, como em `UsuarioLoginResponse`, que também retorna o token JWT.

## Contratos da API

Os DTOs de resposta devem retornar apenas os campos necessários para o contrato do endpoint. Quando a especificação não pedir um campo, objeto aninhado ou relacionamento completo, a decisão do projeto é não incluir esse dado automaticamente.

No CRUD de postagens, o documento especifica que a postagem deve estar vinculada a um usuário e a um tema, mas não especifica que a resposta deve retornar os objetos completos de usuário e tema. Por isso, `PostagemResponse` retorna apenas `UsuarioId` e `TemaId`. Essa decisão foi tomada para evitar desperdício de código, processamento e envio desnecessário de dados.

O projeto gera documentação XML para que o Swagger exiba os comentários escritos com `/// <summary>`. O warning `CS1591` foi desativado no `.csproj` porque nem toda classe, método ou propriedade pública precisa de comentário XML; a documentação deve ser usada apenas onde melhora o contrato da API.

## Integração com IA

Ao cadastrar uma postagem, a API está preparada para enviar o conteúdo para um provedor externo de IA e salvar na postagem o resumo, a categoria e as tags retornadas.

O provedor ainda não está definido. A integração fica controlada pelas variáveis genéricas `IA_ENABLED`, `IA_PROVIDER`, `IA_BASE_URL`, `IA_API_KEY` e `IA_MODEL`. No Docker Compose, esses valores são enviados para a aplicação como `IA__Enabled`, `IA__Provider`, `IA__BaseUrl`, `IA__ApiKey` e `IA__Model`.

Enquanto `IA_ENABLED=false`, o cadastro de postagens continua funcionando e os campos `ResumoIA`, `TagsIA` e `CategoriaIA` ficam vazios. Quando o provedor for escolhido, a implementação específica deve ser conectada à interface `IIAProvider`.

Também existe a rota protegida `POST /api/ia/resumir`, usada para gerar resumo, categoria e tags a partir de um texto sem criar uma postagem.

Se a integração estiver desabilitada ou sem provedor configurado, essa rota retorna erro de integração com IA em vez de chamar uma API externa.

## Autenticação JWT

O token JWT do usuário guarda apenas dados úteis para identificação e autorização. Usamos o `Id` como identificador principal do usuário autenticado, o `Email` e o `Nome` como informações de contexto, e o `Tipo` como perfil de acesso para permitir regras futuras como rotas restritas a administradores.

Dados sensíveis, como senha ou hash da senha, não devem entrar no token. O objetivo é manter o JWT suficiente para validar a identidade do usuário sem transformar o token em uma cópia completa do cadastro.

## Regras de Acesso

As rotas de cadastro, login, listagem de temas e listagem/filtro de postagens são públicas. As rotas que alteram cadastros existentes, postagens, temas ou que consomem IA exigem autenticação.

Usuários podem atualizar e excluir o próprio cadastro. Administradores podem excluir qualquer usuário, mas não podem alterar dados pessoais de outros usuários.

A regra de negócio adotada é que o cadastro público sempre cria usuários com o perfil `Usuario`. O perfil também não pode ser alterado pela rota comum de atualização de cadastro, porque alteração de privilégio é uma ação administrativa separada.

Por isso, a API possui a rota `PATCH /api/usuarios/{id}/privilegio`, restrita a administradores. Essa rota aceita apenas `Usuario` ou `Admin` e não permite que o administrador autenticado altere o próprio perfil.

Em ambiente de desenvolvimento, os seeders criam um administrador padrão para permitir o primeiro acesso administrativo:

```text
Email: admin@blogsharp.com
Senha: Admin@123
```

Como o perfil de acesso fica gravado no JWT, a alteração de privilégio passa a valer no próximo login do usuário alterado. Tokens já emitidos continuam carregando o perfil antigo até expirarem.

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

Os testes ficam em `tests/BlogSharp.Api.Tests`.

Os testes unitários cobrem as regras principais do `UsuarioService`, `TemaService` e `PostagemService`, usando fakes simples para repositories e token.

Os testes de integração sobem a API em memória e validam fluxos HTTP principais, como cadastro, login, autorização JWT, criação de temas por administrador e criação/filtro de postagens.

Para executar todos os testes, unitários e de integração:

```bash
dotnet test BlogSharp.sln
```

Para executar apenas os testes de integração:

```bash
dotnet test BlogSharp.sln --filter FullyQualifiedName~Integration
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

O profile `quality` é apenas uma separação operacional do Docker Compose. Ele não representa uma obrigação de cumprir o Quality Gate padrão do SonarQube.

O PDF do desafio pede configuração do SonarQube, integração com o build, uso das métricas e geração de relatórios. Ele não define meta mínima de cobertura, não exige o Quality Gate `Sonar Way` e não pede aprovação obrigatória em 80% de cobertura.

Por isso, neste projeto o SonarQube será usado como ferramenta de inspeção. O foco é corrigir problemas reais apontados pela análise, como bugs, vulnerabilidades, hotspots relevantes e más práticas. A cobertura continuará disponível como métrica auxiliar, mas não será usada para criar testes artificiais apenas para atingir porcentagem.

### 1. Subir o SonarQube

Na raiz do projeto, execute:

```bash
docker compose --profile quality up -d sonarqube
```

O painel ficará disponível em:

```text
http://localhost:9000
```

No primeiro acesso, use o login padrão `admin` e senha `admin`. O SonarQube irá solicitar a troca da senha.

Esta configuração usa o banco interno do SonarQube, suficiente para ambiente local de estudo e análise inicial.

### 2. Configurar o token

O token do SonarQube deve ficar apenas no `.env` local, na variável `SONAR_TOKEN`. Essa decisão evita colocar segredo em arquivo versionado.

```env
SONAR_TOKEN=seu_token_do_sonarqube
```

### 3. Instalar o scanner

A análise não é executada dentro do container do SonarQube. O container `sonarqube` funciona como servidor e painel web. Quem analisa o código é o `dotnet-sonarscanner`, executado na máquina local junto com o build do projeto.

Caso ainda não tenha o scanner instalado:

```bash
dotnet tool install --global dotnet-sonarscanner
```

Se o terminal não reconhecer `dotnet sonarscanner`, feche e abra o terminal ou adicione as ferramentas globais ao `PATH`:

```bash
export PATH="$PATH:$HOME/.dotnet/tools"
```

### 4. Executar a análise

O projeto usa o fluxo oficial do `dotnet-sonarscanner` para aplicações .NET: `begin`, `build`, testes com cobertura e `end`. As configurações da análise ficam no script `scripts/sonar.sh`, em vez de `sonar-project.properties`, porque o scanner .NET não usa esse arquivo.

Durante a análise, o script executa os testes com Coverlet e gera um relatório OpenCover em `.sonarqube/coverage/coverage.opencover.xml`. Esse relatório é enviado ao SonarQube para preencher a métrica de cobertura.

Para consultar a cobertura por pasta e arquivo sem depender da navegação manual no SonarQube:

```bash
./scripts/coverage-report.sh
```

O relatório local será gerado em `.sonarqube/coverage/coverage-report.md`.

Para rodar:

```bash
./scripts/sonar.sh
```

Ao final da execução, o relatório fica disponível em:

```text
http://localhost:9000/dashboard?id=BlogSharp
```

### Hotspot do Dockerfile

O SonarQube aponta um hotspot no `Dockerfile` porque a imagem base do SDK .NET pode executar como `root` quando usada isoladamente.

No fluxo atual do projeto, esse risco foi revisado como seguro para o ambiente local, pois a API é executada pelo Docker Compose com o usuário do host:

```yaml
user: "${HOST_UID:-1000}:${HOST_GID:-1000}"
```

Essa decisão evita alterar o Dockerfile de desenvolvimento sem necessidade e mantém o funcionamento do `dotnet watch` com volume montado. Caso seja criado um Dockerfile de produção, a imagem deve definir um usuário não-root diretamente.

## Licença

Este projeto está licenciado conforme o arquivo `LICENSE`.
