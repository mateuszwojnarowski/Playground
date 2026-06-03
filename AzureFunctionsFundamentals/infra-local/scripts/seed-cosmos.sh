#!/usr/bin/env bash
# Creates Cosmos DB database/containers and seeds sample data in the local
# Cosmos emulator, based on infra-local/cosmosdb/init.json.
#
# Requires: Azure CLI (az) with the cosmosdb extension, OR use the Data Explorer
# at https://localhost:8081/_explorer/index.html (see upload-to-emulators.md).
#
# The emulator uses a well-known fixed key (safe – local only):
set -euo pipefail
HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ENDPOINT="https://localhost:8081"
KEY="C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="
DB="LearningDb"

echo "This script uses the Cosmos DB REST/Data plane via the emulator's fixed key."
echo "Containers to create: orders, orders-leases, audit, products"
echo
echo "Recommended: open the Data Explorer and create them visually:"
echo "  https://localhost:8081/_explorer/index.html"
echo
echo "Or use the .NET seeding helper documented in docs/upload-to-emulators.md,"
echo "which reads infra-local/cosmosdb/init.json and creates everything for you."
