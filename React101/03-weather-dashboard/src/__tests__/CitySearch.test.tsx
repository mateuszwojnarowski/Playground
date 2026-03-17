import { render, screen, fireEvent } from '@testing-library/react';
import { CitySearch } from '../components/CitySearch';
import { useCitySearch } from '../hooks/useCitySearch';

vi.mock('../hooks/useCitySearch');

describe('CitySearch', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('renders search input', () => {
    vi.mocked(useCitySearch).mockReturnValue({
      query: '',
      setQuery: vi.fn(),
      results: [],
      isSearching: false,
    });

    render(<CitySearch onSelect={vi.fn()} />);
    expect(screen.getByPlaceholderText('Search for a city...')).toBeInTheDocument();
  });

  it('shows results when the hook returns them', () => {
    vi.mocked(useCitySearch).mockReturnValue({
      query: 'London',
      setQuery: vi.fn(),
      results: [
        {
          id: 1,
          name: 'London',
          country: 'United Kingdom',
          admin1: 'England',
          latitude: 51.5,
          longitude: -0.12,
        },
      ],
      isSearching: false,
    });

    render(<CitySearch onSelect={vi.fn()} />);

    expect(screen.getByText('London')).toBeInTheDocument();
    expect(screen.getByText('England, United Kingdom')).toBeInTheDocument();
  });

  it('calls onSelect when a result is clicked', () => {
    const city = {
      id: 1,
      name: 'Paris',
      country: 'France',
      latitude: 48.85,
      longitude: 2.35,
    };
    const setQuery = vi.fn();
    vi.mocked(useCitySearch).mockReturnValue({
      query: 'Paris',
      setQuery,
      results: [city],
      isSearching: false,
    });

    const onSelect = vi.fn();
    render(<CitySearch onSelect={onSelect} />);

    fireEvent.click(screen.getByText('Paris'));

    expect(onSelect).toHaveBeenCalledWith(city);
    expect(setQuery).toHaveBeenCalledWith('');
  });
});
