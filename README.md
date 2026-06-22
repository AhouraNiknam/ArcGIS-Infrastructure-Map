# ArcGIS-Infrastructure-Map

A prototype tool for cataloging and visualizing public infrastructure — levees, leveed areas, pump stations, and dams — within a given radius of any point in the United States. Built as a proof-of-concept using live federal geospatial data from the U.S. Army Corps of Engineers.

## What it does

Click a point on the map, set a search radius (in kilometers), and the app queries real federal infrastructure datasets and draws every matching asset on the map, color-coded by type. Click any feature to see its full details in the side panel.

- **Leveed Areas** (orange)
- **Pump Stations** (green)
- **Dams** (red)

All data comes from public ArcGIS REST services — no API keys or paid accounts required.

## Architecture

The project is split into two independent parts that communicate over HTTP:

**Backend** — an ASP.NET Core (.NET 9) minimal Web API in C#. It exposes a single endpoint, `/infrastructure`, that accepts a latitude, longitude, and radius. It fans the request out across multiple USACE ArcGIS Feature Services in parallel, merges the results, and returns them as GeoJSON. The spatial radius query is performed server-side by ArcGIS, so the backend stays a thin orchestration layer.

**Frontend** — a single self-contained `index.html` page using the ArcGIS Maps SDK for JavaScript. It handles the map, the click-to-search interaction, the radius buffer, color-coding by type, and the click-to-inspect details panel. No build step required.

This split mirrors how the eventual product would likely be shaped — a browser-accessed tool backed by a service — so the prototype work isn't throwaway.

## Data sources

All datasets are public services published by the U.S. Army Corps of Engineers:

- **National Levee Database (NLD)** — leveed areas, pump stations
- **National Inventory of Dams (NID)** — dams

## Running it locally

You need both parts running at once.

### 1. Start the backend

Open the solution in Visual Studio 2022 and run it (F5), or from the command line:

```
dotnet run
```

Note the address it prints, e.g. `Now listening on: http://localhost:5085`. The port number is generated per-machine — yours may differ.

### 2. Point the frontend at your backend

Open `index.html` and find this line near the top of the `<script>` section:

```javascript
const BACKEND = "http://localhost:5085";
```

Change the port to match whatever your backend is listening on. **This is the most common reason a fresh clone "doesn't work" — the ports must match.**

### 3. Open the map

Open `index.html` in any modern browser (double-click it, or right-click → Open With). The map loads and calls your running backend behind the scenes.

### Try it

- **Levees:** the map opens on New Orleans — click near the city and search.
- **Dams:** search near Hoover Dam (lat `36.016`, lon `-114.737`).

## Project structure

```
Program.cs                       App setup and the /infrastructure endpoint
Models.cs                        Data shapes (records) passed through the app
ArcGisInfrastructureService.cs   Queries the ArcGIS services and merges results
GeoMath.cs                       Bounding-box utility (for future local caching)
index.html                       The map frontend
```

## Caveats

This is a proof-of-concept, not production software. Known shortcuts, left intentional and visible:

- **CORS is wide open** (`AllowAnyOrigin`) so the local HTML page can call the API. Tighten before any real deployment.
- **Result caps:** each ArcGIS service returns at most ~2,000 features per query. Very large radii may silently hit this cap. Production use would need pagination.
- **GeoJSON-to-map conversion** is done manually for clarity. The SDK's native `GeoJSONLayer` and popups would be the production approach.
- The `.mil` data services occasionally respond slowly or are briefly unavailable; the app degrades gracefully per-layer when one fails.
