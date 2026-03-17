import { render, screen } from '@testing-library/react';
import { CurrentWeather } from '../components/CurrentWeather';

describe('CurrentWeather', () => {
  const weather = {
    temperature: 22.5,
    humidity: 65,
    windSpeed: 12.3,
    weatherCode: 0,
  };

  const location = {
    id: 1,
    name: 'Berlin',
    country: 'Germany',
    admin1: 'Land Berlin',
    latitude: 52.52,
    longitude: 13.405,
  };

  it('renders location name', () => {
    render(<CurrentWeather weather={weather} location={location} />);
    expect(screen.getByText('Berlin')).toBeInTheDocument();
  });

  it('renders country and state', () => {
    render(<CurrentWeather weather={weather} location={location} />);
    expect(screen.getByText('Land Berlin, Germany')).toBeInTheDocument();
  });

  it('renders temperature', () => {
    render(<CurrentWeather weather={weather} location={location} />);
    expect(screen.getByText('23°C')).toBeInTheDocument();
  });

  it('renders humidity', () => {
    render(<CurrentWeather weather={weather} location={location} />);
    expect(screen.getByText('65%')).toBeInTheDocument();
  });

  it('renders wind speed', () => {
    render(<CurrentWeather weather={weather} location={location} />);
    expect(screen.getByText('12.3 km/h')).toBeInTheDocument();
  });

  it('renders weather description', () => {
    render(<CurrentWeather weather={weather} location={location} />);
    expect(screen.getByText('Clear sky')).toBeInTheDocument();
  });
});
