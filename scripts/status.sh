#!/usr/bin/env bash
# status.sh — Check health of all Ship24X7 backend services
# Usage: ./scripts/status.sh

GREEN='\033[0;32m'; RED='\033[0;31m'; NC='\033[0m'

echo ""
echo "  Service        Port   Health"
echo "  ─────────────────────────────────────"

for entry in "auth:9001" "shipment:9002" "tracking:9003" "notification:9004" "payment:9005" "gateway:8000"; do
  name="${entry%%:*}"
  port="${entry##*:}"
  if curl -sf --max-time 3 "http://localhost:$port/health" > /dev/null 2>&1; then
    echo -e "  ${GREEN}✓${NC}  $(printf '%-14s' $name)  $port   healthy"
  else
    echo -e "  ${RED}✗${NC}  $(printf '%-14s' $name)  $port   unreachable"
  fi
done

echo ""
