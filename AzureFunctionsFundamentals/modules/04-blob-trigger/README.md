# Module 04: Blob Trigger

## Concept

Blob triggers run a function when a blob is created or updated. They are a natural fit for reacting to uploads such as CSV exports, images, or JSON documents. For very high-scale storage accounts, consider Event Grid-based blob triggers because Event Grid avoids some polling costs and latency.

## Scenario

The exercise watches the `uploads` container for CSV order files, transforms the uploaded content into normalized JSON, and writes the result to the `processed` container with a blob output binding.

## Acceptance criteria

- [ ] Blob trigger listens to `uploads/{name}` using `AzureWebJobsStorage`.
- [ ] Processed output is written to the `processed` container.
- [ ] Transformation logic lives in `UploadProcessor`, not in the Function method.
- [ ] Unit tests feed sample input directly to the processor without the Functions runtime or Azurite.

## Run locally

Start Azurite from `infra-local`, then run the exercise project:

```bash
cd AzureFunctionsFundamentals/modules/04-blob-trigger/exercise
dotnet run
```

Upload a CSV file to Azurite:

```bash
az storage blob upload \
  --container-name uploads \
  --name orders.csv \
  --file ./orders.csv \
  --connection-string 'UseDevelopmentStorage=true'
```

Example CSV:

```csv
id,customerId,product,quantity,unitPrice
order-2001,42,Keyboard,2,49.99
order-2002,7,Mouse,1,25.50
```

## Tests

```bash
cd AzureFunctionsFundamentals/modules/04-blob-trigger/exercise/tests
dotnet test
```
