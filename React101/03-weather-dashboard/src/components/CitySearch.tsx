import { useRef, useEffect } from 'react';
import type { GeocodingResult } from '../types/weather';
import { useCitySearch } from '../hooks/useCitySearch';

interface CitySearchProps {
  onSelect: (city: GeocodingResult) => void;
}

export function CitySearch({ onSelect }: CitySearchProps) {
  const { query, setQuery, results, isSearching } = useCitySearch();
  const containerRef = useRef<HTMLDivElement>(null);
  const showDropdown = results.length > 0 || isSearching;

  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setQuery('');
      }
    }
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [setQuery]);

  function handleSelect(city: GeocodingResult) {
    setQuery('');
    onSelect(city);
  }

  return (
    <div className="city-search" ref={containerRef}>
      <div className="city-search__input-wrapper">
        <span className="city-search__icon">🔍</span>
        <input
          type="text"
          className="city-search__input"
          placeholder="Search for a city..."
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          aria-label="Search for a city"
        />
      </div>

      {showDropdown && (
        <ul className="city-search__dropdown" role="listbox">
          {isSearching && results.length === 0 && (
            <li className="city-search__item city-search__item--loading">Searching...</li>
          )}
          {results.map((city) => (
            <li
              key={city.id}
              className="city-search__item"
              role="option"
              aria-selected={false}
              onClick={() => handleSelect(city)}
            >
              <span className="city-search__city-name">{city.name}</span>
              <span className="city-search__city-detail">
                {city.admin1 ? `${city.admin1}, ` : ''}
                {city.country}
              </span>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
