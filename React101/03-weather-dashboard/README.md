# Section 03 – Weather Dashboard

A real-world React application that connects to the [Open-Meteo](https://open-meteo.com/) weather API to display current conditions and a 7-day forecast for any city worldwide. No API key required.

## What You'll Learn

- **API Integration** – Fetching data from REST APIs with `fetch`, transforming responses, and handling errors
- **Custom Hooks** – Extracting reusable stateful logic (`useWeather`, `useCitySearch`)
- **Component Composition** – Building a UI from small, focused components
- **Loading & Error States** – Providing feedback during async operations
- **Debouncing** – Delaying API calls while the user types to reduce network requests
- **TypeScript** – Defining interfaces for API data and component props

## Getting Started

```bash
npm install
npm run dev
```

Open [http://localhost:5173](http://localhost:5173) and search for a city.

### Run Tests

```bash
npm test            # single run
npm run test:watch  # watch mode
```

## Architecture

```
src/
├── api/
│   └── weatherApi.ts          # API functions (fetch + transform)
├── components/
│   ├── CitySearch.tsx          # Search input with autocomplete dropdown
│   ├── CurrentWeather.tsx      # Current conditions card
│   ├── DailyForecast.tsx       # 7-day forecast list
│   ├── WeatherIcon.tsx         # Weather code → emoji mapping
│   └── ErrorMessage.tsx        # Reusable error display with retry
├── hooks/
│   ├── useWeather.ts           # Fetches weather for given coordinates
│   └── useCitySearch.ts        # Debounced city search
├── types/
│   └── weather.ts              # TypeScript interfaces
├── utils/
│   └── weatherCodes.ts         # WMO weather code descriptions & icons
├── __tests__/                  # Test files
├── App.tsx                     # Main layout
├── App.css                     # Component styles
├── index.css                   # Global styles & CSS variables
└── main.tsx                    # Entry point
```

### Why This Structure?

| Layer | Purpose |
|---|---|
| `api/` | Isolates network calls so components never call `fetch` directly |
| `hooks/` | Encapsulates async state management and side effects |
| `types/` | Single source of truth for data shapes |
| `utils/` | Pure functions with no side effects – easy to test |
| `components/` | Presentational components that receive data via props |

## Key Patterns

### Custom Hooks

`useWeather` and `useCitySearch` follow the same pattern:
1. Accept inputs (coordinates, search query)
2. Manage loading, data, and error state internally
3. Return a clean interface for the component to consume

### API Layer Separation

Raw API responses are transformed into clean TypeScript types at the boundary (`weatherApi.ts`), so the rest of the app works with well-typed domain objects.

### Debouncing

`useCitySearch` delays the geocoding API call by 300 ms after the user stops typing, preventing excessive network requests on every keystroke.

## API Reference

This app uses two free Open-Meteo endpoints (no authentication needed):

### Geocoding

```
GET https://geocoding-api.open-meteo.com/v1/search?name={city}&count=5
```

Returns matching cities with coordinates.

### Weather Forecast

```
GET https://api.open-meteo.com/v1/forecast
    ?latitude={lat}&longitude={lon}
    &current=temperature_2m,relative_humidity_2m,wind_speed_10m,weather_code
    &daily=weather_code,temperature_2m_max,temperature_2m_min
    &timezone=auto
```

Returns current conditions and a 7-day daily forecast.

## Exercises

Try extending the app to practice more React patterns:

1. **Recent Searches** – Save selected cities to `localStorage` and show them as quick-select chips
2. **Temperature Units** – Add a Celsius / Fahrenheit toggle using React state or context
3. **Hourly Forecast** – Fetch and display the next 24 hours of hourly data
4. **Weather Animations** – Render CSS or canvas animations based on the current weather code

## Concepts Demonstrated

- Functional components and JSX
- `useState` and `useEffect` hooks
- Custom hooks for shared logic
- `useRef` for DOM references and timers
- Controlled inputs
- Conditional rendering
- List rendering with `key`
- Event handling (`onChange`, `onClick`, `mousedown`)
- Asynchronous data fetching with `fetch`
- Error handling with try/catch
- TypeScript interfaces and generics
- CSS custom properties (variables)
- Responsive design with media queries
- Component composition and prop passing
- Cleanup functions in `useEffect`
