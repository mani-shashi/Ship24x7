#!/usr/bin/env bash
# start.sh — Start all Ship24X7 backend services
# Usage: ./scripts/start.sh

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
LOGS="$ROOT/logs"
mkdir -p "$LOGS"

GREEN='\033[0;32m'; YELLOW='\033[1;33m'; RED='\033[0;31m'; NC='\033[0m'

start() {
  local name=$1 project=$2 port=$3
  echo -e "${GREEN}[start]${NC} $name → port $port"
  dotnet run --project "$ROOT/$project" --no-launch-profile --no-restore \
    > "$LOGS/$name.log" 2>&1 &
}

start auth         src/Services/Auth/Ship24X7.Auth.API/Ship24X7.Auth.API.csproj                           9001
start shipment     src/Services/Shipment/Ship24X7.Shipment.API/Ship24X7.Shipment.API.csproj               9002
start tracking     src/Services/Tracking/Ship24X7.Tracking.API/Ship24X7.Tracking.API.csproj               9003
start notification src/Services/Notification/Ship24X7.Notification.API/Ship24X7.Notification.API.csproj  9004
start payment      src/Services/Payment/Ship24X7.Payment.API/Ship24X7.Payment.API.csproj                  9005
start gateway      src/Gateway/Ship24X7.Gateway/Ship24X7.Gateway.csproj                                   8000

echo ""

# Run status.sh every 5s until all healthy, max 3 attempts
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
MAX=5
attempt=0
all_healthy=false

while [[ $attempt -lt $MAX ]]; do
  attempt=$((attempt + 1))
  echo -e "${YELLOW}Health check attempt $attempt/$MAX ...${NC}"
  sleep 5

  # Capture status output and check for any ✗
  output=$("$SCRIPT_DIR/status.sh")
  echo "$output"

  if ! echo "$output" | grep -q "✗"; then
    all_healthy=true
    break
  fi

  [[ $attempt -lt $MAX ]] && echo ""
done

echo ""
if $all_healthy; then
  echo -e "${GREEN}All services are up.${NC}"
else
  echo -e "${RED}Some services did not become healthy after $MAX attempts. Check logs/ for details.${NC}"
fi

echo ""
echo "  Gateway  → http://localhost:8000"
echo "  Logs     → $LOGS/"
echo ""
