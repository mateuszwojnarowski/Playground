import { searchCities, fetchWeather } from '../api/weatherApi';

const originalFetch = globalThis.fetch;

afterEach(() => {
  globalThis.fetch = originalFetch;
});

describe('searchCities', () => {
  it('returns transformed geocoding results', async () => {
    globalThis.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: () =>
        Promise.resolve({
          results: [
            {
              id: 2950159,
              name: 'Berlin',
              country: 'Germany',
              admin1: 'Land Berlin',
              latitude: 52.52,
              longitude: 13.405,
            },
          ],
        }),
    });

    const results = await searchCities('Berlin');

    expect(results).toHaveLength(1);
    expect(results[0]).toEqual({
      id: 2950159,
      name: 'Berlin',
      country: 'Germany',
      admin1: 'Land Berlin',
      latitude: 52.52,
      longitude: 13.405,
    });
    expect(globalThis.fetch).toHaveBeenCalledWith(
      expect.stringContaining('name=Berlin'),
    );
  });

  it('returns empty array when no results', async () => {
    globalThis.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({}),
    });

    const results = await searchCities('xyznonexistent');
    expect(results).toEqual([]);
  });

  it('returns empty array for empty query', async () => {
    const results = await searchCities('');
    expect(results).toEqual([]);
  });

  it('throws on failed request', async () => {
    globalThis.fetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 500,
      statusText: 'Internal Server Error',
    });

    await expect(searchCities('Berlin')).rejects.toThrow(
      'Geocoding request failed: 500 Internal Server Error',
    );
  });
});

describe('fetchWeather', () => {
  it('returns transformed weather data', async () => {
    globalThis.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: () =>
        Promise.resolve({
          current: {
            temperature_2m: 22.5,
            relative_humidity_2m: 65,
            wind_speed_10m: 12.3,
            weather_code: 1,
          },
          daily: {
            time: ['2024-01-15', '2024-01-16'],
            weather_code: [0, 3],
            temperature_2m_max: [24.0, 20.0],
            temperature_2m_min: [15.0, 12.0],
          },
        }),
    });

    const data = await fetchWeather(52.52, 13.405);

    expect(data.current).toEqual({
      temperature: 22.5,
      humidity: 65,
      windSpeed: 12.3,
      weatherCode: 1,
    });
    expect(data.daily).toHaveLength(2);
    expect(data.daily[0]).toEqual({
      date: '2024-01-15',
      maxTemp: 24.0,
      minTemp: 15.0,
      weatherCode: 0,
    });
  });

  it('throws on failed request', async () => {
    globalThis.fetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 404,
      statusText: 'Not Found',
    });

    await expect(fetchWeather(0, 0)).rejects.toThrow(
      'Weather request failed: 404 Not Found',
    );
  });
});
