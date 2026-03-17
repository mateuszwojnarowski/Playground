import type { DailyForecast as DailyForecastData } from '../types/weather';
import { WeatherIcon } from './WeatherIcon';

interface DailyForecastProps {
  forecast: DailyForecastData[];
}

function formatDay(dateStr: string): string {
  const date = new Date(dateStr + 'T00:00:00');
  const today = new Date();
  today.setHours(0, 0, 0, 0);

  const tomorrow = new Date(today);
  tomorrow.setDate(tomorrow.getDate() + 1);

  if (date.getTime() === today.getTime()) return 'Today';
  if (date.getTime() === tomorrow.getTime()) return 'Tomorrow';

  return date.toLocaleDateString('en-US', { weekday: 'short', month: 'short', day: 'numeric' });
}

export function DailyForecast({ forecast }: DailyForecastProps) {
  return (
    <div className="daily-forecast">
      <h3 className="daily-forecast__title">7-Day Forecast</h3>
      <div className="daily-forecast__list">
        {forecast.map((day) => (
          <div key={day.date} className="daily-forecast__day">
            <span className="daily-forecast__day-name">{formatDay(day.date)}</span>
            <WeatherIcon code={day.weatherCode} size="small" />
            <span className="daily-forecast__temps">
              <span className="daily-forecast__max">{Math.round(day.maxTemp)}°</span>
              <span className="daily-forecast__min">{Math.round(day.minTemp)}°</span>
            </span>
          </div>
        ))}
      </div>
    </div>
  );
}
