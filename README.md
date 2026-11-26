# Workshop Chat API

ASP.NET Core web application and API for campus workshops, workshop detail updates, and workshop-scoped chat persistence in SQLite.

## Features
- Workshop catalog and detail editing (`seatsLeft`, description, metadata) synced with SQLite (`campus.db`)
- Workshop-scoped chat messages filtered by workshop ID and URL hash (`#workshop-1`, `#workshop-2`)
- REST endpoints for workshops (`/api/workshops/{id}`) and chat (`/api/chat?workshopId=...`)

## Run Locally
1. Start the server using the HTTP launch profile:
   `dotnet run --launch-profile http`
2. Open the app in your browser at `http://localhost:5024`.
