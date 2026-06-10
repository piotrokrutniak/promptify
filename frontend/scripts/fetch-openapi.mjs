import { mkdir, writeFile } from "node:fs/promises"
import path from "node:path"
import { fileURLToPath } from "node:url"

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const rootDir = path.resolve(__dirname, "..")
const outputPath = path.join(rootDir, "openapi", "v1.json")

const openApiUrl =
  process.env.OPENAPI_URL ?? "http://localhost:8080/openapi/v1.json"

async function fetchOpenApiSpec() {
  let response

  try {
    response = await fetch(openApiUrl)
  } catch (error) {
    const message = error instanceof Error ? error.message : String(error)
    console.error(`Failed to fetch OpenAPI spec from ${openApiUrl}: ${message}`)
    console.error(
      "Ensure the backend is running (make docker-up or make apphost) and OPENAPI_URL is correct.",
    )
    process.exit(1)
  }

  if (!response.ok) {
    console.error(
      `Failed to fetch OpenAPI spec from ${openApiUrl}: HTTP ${response.status} ${response.statusText}`,
    )
    console.error(
      "Ensure the backend is running (make docker-up or make apphost) and OPENAPI_URL is correct.",
    )
    process.exit(1)
  }

  const spec = await response.json()

  await mkdir(path.dirname(outputPath), { recursive: true })
  await writeFile(outputPath, `${JSON.stringify(spec, null, 2)}\n`, "utf8")

  console.log(`Wrote OpenAPI spec to ${path.relative(rootDir, outputPath)}`)
}

await fetchOpenApiSpec()
