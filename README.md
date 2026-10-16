# Lab8ApiImplementation (back-end API)

ASP.NET Core (.NET 10) Web API with a Groq chat endpoint and a RAG endpoint.
The React front-end lives in its own repo: **Lab8ApiImplementation-client**.

## Endpoints

| Method | Path           | Description                         |
|--------|----------------|-------------------------------------|
| POST   | `/api/chat`    | Chat with the Groq model            |
| POST   | `/api/rag/ask` | Answer questions from `Data/Docs`   |
| GET    | `/api/crash`   | Fails on purpose (tests error middleware) |
| GET    | `/health`      | Health check                        |
| GET    | `/swagger`     | Swagger UI                          |

## Run locally

```bash
cd Lab8ApiImplementation
dotnet user-secrets set GROQ_API_KEY "<your key>"
dotnet run --launch-profile http     # http://localhost:5079
```

Then start the front-end repo (`npm run dev`, http://localhost:5173).

## Configuration

| Setting | Where | Example |
|---------|-------|---------|
| `GROQ_API_KEY` | user-secrets locally, environment variable in production | `gsk_...` |
| `Cors:AllowedOrigins` | `appsettings.json` locally; environment variables in production | `Cors__AllowedOrigins__0=https://my-frontend.example.com` |

The front-end URL must be listed in `Cors:AllowedOrigins`, otherwise the browser blocks the requests.
Use the exact origin: scheme + host (+ port), no trailing slash.

## Deploy (e.g. Azure App Service, Render, Railway)

1. `dotnet publish Lab8ApiImplementation -c Release -o publish` (or let the host build it).
2. Set environment variables on the host:
   - `GROQ_API_KEY=<your key>`
   - `Cors__AllowedOrigins__0=https://<your-frontend-domain>`
3. Use HTTPS. A front-end served over HTTPS cannot call an `http://` API.
4. Check `https://<your-api-domain>/health`.
