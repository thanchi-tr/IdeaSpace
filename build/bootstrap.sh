#!/bin/bash

# Enable strict mode
set -euo pipefail
IFS=$'\n\t'

# Define constants
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
COMPOSE_FILE_CORE="$PROJECT_ROOT/build/docker-compose.yml"
COMPOSE_FILE_MODULES="$PROJECT_ROOT/build/docker-compose.module.yml"
NETWORK_NAME="ideaspace_network"

echo "[INFO] [Bootstrap] Starting full system bootstrap..."
echo "[INFO] Project Root: $PROJECT_ROOT"

# Step 1: Create shared network if not exists
if ! docker network ls --format '{{.Name}}' | grep -q "^${NETWORK_NAME}$"; then
  echo "[INFO] Creating shared network: $NETWORK_NAME"
  docker network create "$NETWORK_NAME"
else
  echo "[INFO] Shared network '$NETWORK_NAME' already exists"
fi
read -p "[INFO] Press Enter to continue..."
# Step 2: Start core dependencies (Postgres, Redis, RabbitMQ)
echo "[INFO] Launching core services..."
read -p "[INFO] Press Enter to continue..."
docker compose -f "$COMPOSE_FILE_CORE" up -d --build

# Step 3: Start all application modules
echo "[INFO] Launching application modules..."
read -p "[INFO] Press Enter to continue..."
docker compose -f "$COMPOSE_FILE_MODULES" up -d --build


read -p "[INFO] Press Enter to continue..."
# Step 4: Wait for health checks (Postgres + RabbitMQ)
echo "[WAITING]  for core services to become healthy..."
until docker inspect --format='{{.State.Health.Status}}' local-postgres 2>/dev/null | grep -q "healthy" && \
      docker inspect --format='{{.State.Health.Status}}' local-rabbitmq 2>/dev/null | grep -q "healthy"; do
  echo "[WAITING] on health checks..."
  sleep 5
done
sleep 500
echo "[INFO] Bootstrap complete! All systems are live."

# Optional: Show status
docker compose -f "$COMPOSE_FILE_CORE" ps
docker compose -f "$COMPOSE_FILE_MODULES" ps

# Final pause to prevent auto-close
echo ""
read -p "[INFO] Press Enter to exit bootstrap..."
