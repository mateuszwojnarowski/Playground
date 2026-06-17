import { useState, useEffect } from 'react';
import type { CurrentWeather, DailyForecast } from '../types/weather';
import { fetchWeather } from '../api/weatherApi';

interface UseWeatherReturn {
  weather: { current: CurrentWeather; daily: DailyForecast[] } | null;
  isLoading: boolean;
  error: string | null;
}

export function useWeather(
  latitude: number | null,
  longitude: number | null,
): UseWeatherReturn {
  const [weather, setWeather] = useState<UseWeatherReturn['weather']>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (latitude === null || longitude === null) {
      setWeather(null);
      setError(null);
      return;
    }

    let cancelled = false;

    async function load() {
      setIsLoading(true);
      setError(null);
      try {
        const data = await fetchWeather(latitude!, longitude!);
        if (!cancelled) {
          setWeather(data);
        }
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : 'Failed to fetch weather data');
        }
      } finally {
        if (!cancelled) {
          setIsLoading(false);
        }
      }
    }

    load();

    return () => {
      cancelled = true;
    };
  }, [latitude, longitude]);

  return { weather, isLoading, error };
}
