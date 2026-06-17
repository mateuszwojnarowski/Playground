import { getWeatherInfo } from '../utils/weatherCodes';

interface WeatherIconProps {
  code: number;
  size?: 'small' | 'medium' | 'large';
}

export function WeatherIcon({ code, size = 'medium' }: WeatherIconProps) {
  const { description, icon } = getWeatherInfo(code);
  const sizeClass = `weather-icon weather-icon--${size}`;

  return (
    <span className={sizeClass} role="img" aria-label={description} title={description}>
      {icon}
    </span>
  );
}
