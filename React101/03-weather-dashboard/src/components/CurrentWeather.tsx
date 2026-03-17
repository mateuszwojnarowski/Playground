import type { CurrentWeather as CurrentWeatherData, GeocodingResult } from '../types/weather';
import { WeatherIcon } from './WeatherIcon';
import { getWeatherInfo } from '../utils/weatherCodes';

interface CurrentWeatherProps {
  weather: CurrentWeatherData;
  location: GeocodingResult;
}

export function CurrentWeather({ weather, location }: CurrentWeatherProps) {
  const { description } = getWeatherInfo(weather.weatherCode);

  return (
    <div className="current-weather">
      <div className="current-weather__location">
        <h2 className="current-weather__city">{location.name}</h2>
        <span className="current-weather__country">
          {location.admin1 ? `${location.admin1}, ` : ''}
          {location.country}
        </span>
      </div>

      <div className="current-weather__main">
        <WeatherIcon code={weather.weatherCode} size="large" />
        <div className="current-weather__temp">
          <span className="current-weather__temp-value">
            {Math.round(weather.temperature)}°C
          </span>
          <span className="current-weather__description">{description}</span>
        </div>
      </div>

      <div className="current-weather__details">
        <div className="current-weather__detail">
          <span className="current-weather__detail-icon">💧</span>
          <span className="current-weather__detail-label">Humidity</span>
          <span className="current-weather__detail-value">{weather.humidity}%</span>
        </div>
        <div className="current-weather__detail">
          <span className="current-weather__detail-icon">💨</span>
          <span className="current-weather__detail-label">Wind</span>
          <span className="current-weather__detail-value">{weather.windSpeed} km/h</span>
        </div>
      </div>
    </div>
  );
}
