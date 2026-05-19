#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")/.."
export PATH="$PATH:$HOME/.dotnet/tools"

if [ ! -f .env ]; then
  echo "Arquivo .env nao encontrado na raiz do projeto."
  exit 1
fi

set -a
source .env
set +a

if [ -z "${SONAR_TOKEN:-}" ]; then
  echo "Defina SONAR_TOKEN no arquivo .env antes de rodar a analise."
  exit 1
fi

if ! command -v dotnet-sonarscanner >/dev/null 2>&1; then
  echo "dotnet-sonarscanner nao encontrado. Instale com:"
  echo "dotnet tool install --global dotnet-sonarscanner"
  exit 1
fi

dotnet-sonarscanner begin \
  /k:"blogsharp" \
  /n:"BlogSharp" \
  /d:sonar.host.url="http://localhost:9000" \
  /d:sonar.login="$SONAR_TOKEN" \
  /d:sonar.sourceEncoding="UTF-8" \
  /d:sonar.exclusions="**/bin/**,**/obj/**"

dotnet build BlogSharp.sln --no-incremental

dotnet-sonarscanner end /d:sonar.login="$SONAR_TOKEN"
