#!/usr/bin/env bash
# Seeds Azurite with the blob containers and storage queue used by the modules.
# Requires: Azure CLI (az).
set -euo pipefail
# Azurite well-known development connection string (local only):
CONN="DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1"

echo "Creating blob containers: uploads, processed"
az storage container create --name uploads   --connection-string "$CONN" >/dev/null
az storage container create --name processed --connection-string "$CONN" >/dev/null

echo "Creating storage queue: incoming-jobs"
az storage queue create --name incoming-jobs --connection-string "$CONN" >/dev/null

echo "Done. Azurite is seeded."
