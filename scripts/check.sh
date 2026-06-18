#!/usr/bin/env bash
# POSIX twin of check.ps1 (for CI / Linux / macOS). build + tests + coverage + lint + typecheck.
#   scripts/check.sh [--backend] [--frontend] [--coverage 85]
set -uo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
LOGS="$ROOT/.logs"; mkdir -p "$LOGS"
COVERAGE=85; DO_BE=0; DO_FE=0
while [ $# -gt 0 ]; do case "$1" in
  --backend) DO_BE=1;; --frontend) DO_FE=1;; --coverage) COVERAGE="$2"; shift;; esac; shift; done
if [ $DO_BE -eq 0 ] && [ $DO_FE -eq 0 ]; then DO_BE=1; DO_FE=1; fi

FAILED=()
step() { # name logfile cmd...
  local name="$1" log="$2"; shift 2
  printf '  - %-32s' "$name"
  if "$@" >>"$log" 2>&1; then echo "PASS"; else echo "FAIL -> $log"; FAILED+=("$name"); fi
}

if [ $DO_BE -eq 1 ]; then
  echo; echo "Backend (.NET 10)"
  SLN="$ROOT/backend/TwitterClone.slnx"
  [ -f "$SLN" ] || SLN="$ROOT/backend/TwitterClone.sln"
  if [ ! -f "$SLN" ]; then echo "  - skipped: backend not scaffolded yet"; else
    step "build (warnings as errors)" "$LOGS/backend-build.log" dotnet build "$SLN" -warnaserror --nologo
    step "format (lint)" "$LOGS/backend-format.log" dotnet format "$SLN" --verify-no-changes
    step "tests + coverage >= ${COVERAGE}%" "$LOGS/backend-test.log" \
      dotnet test "$SLN" --nologo /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura \
      /p:Threshold="$COVERAGE" /p:ThresholdType=line /p:ThresholdStat=total
  fi
fi

if [ $DO_FE -eq 1 ]; then
  echo; echo "Frontend (Vue 3)"
  if [ ! -f "$ROOT/frontend/package.json" ]; then echo "  - skipped: frontend not scaffolded yet"; else
    pushd "$ROOT/frontend" >/dev/null
    [ -d node_modules ] || step "npm install" "$LOGS/frontend-install.log" npm install
    step "lint" "$LOGS/frontend-lint.log" npm run lint
    step "type-check" "$LOGS/frontend-tsc.log" npm run type-check
    step "unit tests" "$LOGS/frontend-test.log" npm run test:unit
    popd >/dev/null
  fi
fi

echo
if [ ${#FAILED[@]} -gt 0 ]; then echo "CHECK FAILED (${#FAILED[@]}): ${FAILED[*]}"; exit 1; fi
echo "CHECK PASSED"; exit 0
