export interface GeocodingResult {
  id: number;
  name: string;
  country: string;
  admin1?: string;
  latitude: number;
  longitude: number;
}

export interface CurrentWeather {
  temperature: number;
  humidity: number;
  windSpeed: number;
  weatherCode: number;
}

export interface DailyForecast {
  date: string;
  maxTemp: number;
  minTemp: number;
  weatherCode: number;
}

export interface WeatherData {
  current: CurrentWeather;
  daily: DailyForecast[];
  location: GeocodingResult;
}
