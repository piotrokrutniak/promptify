import type { NextConfig } from "next"

const apiBaseUrl = process.env.API_BASE_URL ?? "http://localhost:8080"

const nextConfig: NextConfig = {
  async rewrites() {
    const hubBase = `${apiBaseUrl.replace(/\/+$/, "")}/hubs/prompts`
    return [
      {
        source: "/hubs/prompts",
        destination: hubBase,
      },
      {
        source: "/hubs/prompts/:path*",
        destination: `${hubBase}/:path*`,
      },
    ]
  },
}

export default nextConfig
