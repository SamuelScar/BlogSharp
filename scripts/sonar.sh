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

COVERAGE_DIR="$(pwd)/.sonarqube/coverage"
COVERAGE_REPORT="$COVERAGE_DIR/coverage.opencover.xml"

dotnet-sonarscanner begin \
  /k:"BlogSharp" \
  /n:"BlogSharp" \
  /d:sonar.host.url="http://localhost:9000" \
  /d:sonar.login="$SONAR_TOKEN" \
  /d:sonar.sourceEncoding="UTF-8" \
  /d:sonar.cs.opencover.reportsPaths="$COVERAGE_REPORT" \
  /d:sonar.exclusions="**/bin/**,**/obj/**"

dotnet build BlogSharp.sln --no-incremental

rm -rf "$COVERAGE_DIR"
mkdir -p "$COVERAGE_DIR"

dotnet test BlogSharp.sln \
  --no-restore \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=opencover \
  /p:CoverletOutput="$COVERAGE_DIR/"

dotnet-sonarscanner end /d:sonar.login="$SONAR_TOKEN"
