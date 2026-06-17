import type { GeocodingResult, CurrentWeather, DailyForecast } from '../types/weather';

const GEOCODING_BASE = 'https://geocoding-api.open-meteo.com/v1/search';
const WEATHER_BASE = 'https://api.open-meteo.com/v1/forecast';

export async function searchCities(query: string): Promise<GeocodingResult[]> {
  if (!query.trim()) return [];

  const url = `${GEOCODING_BASE}?name=${encodeURIComponent(query)}&count=5&language=en`;
  const response = await fetch(url);

  if (!response.ok) {
    throw new Error(`Geocoding request failed: ${response.status} ${response.statusText}`);
  }

  const data: unknown = await response.json();
  const parsed = data as { results?: Array<{
    id: number;
    name: string;
    country: string;
    admin1?: string;
    latitude: number;
    longitude: number;
  }> };

  if (!parsed.results) return [];

  return parsed.results.map((r) => ({
    id: r.id,
    name: r.name,
    country: r.country,
    admin1: r.admin1,
    latitude: r.latitude,
    longitude: r.longitude,
  }));
}

export async function fetchWeather(
  latitude: number,
  longitude: number,
): Promise<{ current: CurrentWeather; daily: DailyForecast[] }> {
  const url =
    `${WEATHER_BASE}?latitude=${latitude}&longitude=${longitude}` +
    `&current=temperature_2m,relative_humidity_2m,wind_speed_10m,weather_code` +
    `&daily=weather_code,temperature_2m_max,temperature_2m_min` +
    `&timezone=auto`;

  const response = await fetch(url);

  if (!response.ok) {
    throw new Error(`Weather request failed: ${response.status} ${response.statusText}`);
  }

  const data: unknown = await response.json();
  const parsed = data as {
    current: {
      temperature_2m: number;
      relative_humidity_2m: number;
      wind_speed_10m: number;
      weather_code: number;
    };
    daily: {
      time: string[];
      weather_code: number[];
      temperature_2m_max: number[];
      temperature_2m_min: number[];
    };
  };

  const current: CurrentWeather = {
    temperature: parsed.current.temperature_2m,
    humidity: parsed.current.relative_humidity_2m,
    windSpeed: parsed.current.wind_speed_10m,
    weatherCode: parsed.current.weather_code,
  };

  const daily: DailyForecast[] = parsed.daily.time.map((date, i) => ({
    date,
    maxTemp: parsed.daily.temperature_2m_max[i],
    minTemp: parsed.daily.temperature_2m_min[i],
    weatherCode: parsed.daily.weather_code[i],
  }));

  return { current, daily };
}
