#!/usr/bin/env bash
# stop.sh — Stop all Ship24X7 backend services
# Usage: ./scripts/stop.sh

GREEN='\033[0;32m'; YELLOW='\033[1;33m'; NC='\033[0m'

PROJECTS=(
  "Ship24X7.Auth.API"
  "Ship24X7.Shipment.API"
  "Ship24X7.Tracking.API"
  "Ship24X7.Notification.API"
  "Ship24X7.Payment.API"
  "Ship24X7.Gateway"
)

echo ""
for proj in "${PROJECTS[@]}"; do
  pids=$(pgrep -f "$proj" 2>/dev/null || true)
  if [[ -n "$pids" ]]; then
    kill $pids
    echo -e "  ${GREEN}stopped${NC}  $proj"
  else
    echo -e "  ${YELLOW}not running${NC}  $proj"
  fi
done
echo ""
