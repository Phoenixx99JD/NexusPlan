#!/usr/bin/env bash
# Aprovisiona la infraestructura de NexusPlan en Azure y configura los secretos de GitHub.
#
# Requisitos: Azure CLI (az login) y, opcionalmente, GitHub CLI (gh auth login).
#
# Uso:
#   APP_NAME=nexusplan-api-<sufijo> SQL_ADMIN_PASSWORD='<Password-Seguro-123!>' ./scripts/azure-provision.sh
set -euo pipefail

APP_NAME="${APP_NAME:?Define APP_NAME (nombre único global, ej. nexusplan-api-jd)}"
SQL_ADMIN_PASSWORD="${SQL_ADMIN_PASSWORD:?Define SQL_ADMIN_PASSWORD}"
SQL_ADMIN_LOGIN="${SQL_ADMIN_LOGIN:-nexusadmin}"
RESOURCE_GROUP="${RESOURCE_GROUP:-rg-nexusplan}"
LOCATION="${LOCATION:-eastus2}"
APP_SKU="${APP_SKU:-B1}"
REPO="${REPO:-Phoenixx99JD/NexusPlan}"

echo ">> Creando grupo de recursos $RESOURCE_GROUP en $LOCATION"
az group create -n "$RESOURCE_GROUP" -l "$LOCATION" -o none

echo ">> Desplegando infraestructura (Bicep)"
az deployment group create \
  -g "$RESOURCE_GROUP" \
  -f "$(dirname "$0")/../infra/main.bicep" \
  -p appName="$APP_NAME" sqlAdminLogin="$SQL_ADMIN_LOGIN" sqlAdminPassword="$SQL_ADMIN_PASSWORD" appServiceSku="$APP_SKU" \
  --query properties.outputs -o json

echo ">> Obteniendo publish profile"
PROFILE=$(az webapp deployment list-publishing-profiles -g "$RESOURCE_GROUP" -n "$APP_NAME" --xml)

if command -v gh >/dev/null 2>&1; then
  echo ">> Configurando secretos/variables en GitHub ($REPO)"
  gh secret set AZURE_WEBAPP_PUBLISH_PROFILE --repo "$REPO" --body "$PROFILE"
  gh variable set AZURE_WEBAPP_NAME --repo "$REPO" --body "$APP_NAME"
else
  echo "!! GitHub CLI no encontrado. Configura manualmente en GitHub > Settings > Secrets and variables > Actions:"
  echo "   - Secret   AZURE_WEBAPP_PUBLISH_PROFILE = (contenido XML del publish profile)"
  echo "   - Variable AZURE_WEBAPP_NAME            = $APP_NAME"
  echo "$PROFILE" > publish-profile.xml
  echo "   Publish profile guardado en ./publish-profile.xml (¡no lo subas al repositorio!)"
fi

echo ">> Listo. API: https://$APP_NAME.azurewebsites.net/swagger"
