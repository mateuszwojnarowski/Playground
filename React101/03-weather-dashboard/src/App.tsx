import { useState } from 'react';
import type { GeocodingResult } from './types/weather';
import { useWeather } from './hooks/useWeather';
import { CitySearch } from './components/CitySearch';
import { CurrentWeather } from './components/CurrentWeather';
import { DailyForecast } from './components/DailyForecast';
import { ErrorMessage } from './components/ErrorMessage';
import './App.css';

function App() {
  const [selectedCity, setSelectedCity] = useState<GeocodingResult | null>(null);
  const { weather, isLoading, error } = useWeather(
    selectedCity?.latitude ?? null,
    selectedCity?.longitude ?? null,
  );

  function handleRetry() {
    if (selectedCity) {
      setSelectedCity({ ...selectedCity });
    }
  }

  return (
    <div className="app">
      <header className="app__header">
        <h1 className="app__title">🌤️ Weather Dashboard</h1>
        <p className="app__subtitle">Search any city for real-time weather</p>
      </header>

      <main className="app__main">
        <CitySearch onSelect={setSelectedCity} />

        {error && <ErrorMessage message={error} onRetry={handleRetry} />}

        {isLoading && (
          <div className="app__loading">
            <div className="spinner" />
            <p>Loading weather data...</p>
          </div>
        )}

        {!isLoading && !error && weather && selectedCity && (
          <>
            <CurrentWeather weather={weather.current} location={selectedCity} />
            <DailyForecast forecast={weather.daily} />
          </>
        )}

        {!isLoading && !error && !weather && (
          <div className="app__empty">
            <span className="app__empty-icon">🌍</span>
            <p>Search for a city to see the weather</p>
          </div>
        )}
      </main>
    </div>
  );
}

export default App;

