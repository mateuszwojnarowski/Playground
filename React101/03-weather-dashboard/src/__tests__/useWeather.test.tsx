import { renderHook, waitFor } from '@testing-library/react';
import { useWeather } from '../hooks/useWeather';
import * as weatherApi from '../api/weatherApi';

vi.mock('../api/weatherApi');

describe('useWeather', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns null weather when coordinates are null', () => {
    const { result } = renderHook(() => useWeather(null, null));
    expect(result.current.weather).toBeNull();
    expect(result.current.isLoading).toBe(false);
    expect(result.current.error).toBeNull();
  });

  it('fetches weather when coordinates are provided', async () => {
    const mockData = {
      current: {
        temperature: 20,
        humidity: 50,
        windSpeed: 10,
        weatherCode: 0,
      },
      daily: [
        { date: '2024-01-15', maxTemp: 22, minTemp: 14, weatherCode: 0 },
      ],
    };

    vi.mocked(weatherApi.fetchWeather).mockResolvedValue(mockData);

    const { result } = renderHook(() => useWeather(52.52, 13.405));

    expect(result.current.isLoading).toBe(true);

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.weather).toEqual(mockData);
    expect(result.current.error).toBeNull();
  });

  it('handles errors', async () => {
    vi.mocked(weatherApi.fetchWeather).mockRejectedValue(
      new Error('Network error'),
    );

    const { result } = renderHook(() => useWeather(52.52, 13.405));

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.error).toBe('Network error');
    expect(result.current.weather).toBeNull();
  });
});
