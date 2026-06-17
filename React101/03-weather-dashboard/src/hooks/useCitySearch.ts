import { useState, useEffect, useRef } from 'react';
import type { GeocodingResult } from '../types/weather';
import { searchCities } from '../api/weatherApi';

interface UseCitySearchReturn {
  query: string;
  setQuery: (value: string) => void;
  results: GeocodingResult[];
  isSearching: boolean;
}

export function useCitySearch(debounceDelay = 300): UseCitySearchReturn {
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<GeocodingResult[]>([]);
  const [isSearching, setIsSearching] = useState(false);
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    if (timerRef.current) {
      clearTimeout(timerRef.current);
    }

    if (!query.trim()) {
      setResults([]);
      setIsSearching(false);
      return;
    }

    setIsSearching(true);

    timerRef.current = setTimeout(async () => {
      try {
        const data = await searchCities(query);
        setResults(data);
      } catch {
        setResults([]);
      } finally {
        setIsSearching(false);
      }
    }, debounceDelay);

    return () => {
      if (timerRef.current) {
        clearTimeout(timerRef.current);
      }
    };
  }, [query, debounceDelay]);

  return { query, setQuery, results, isSearching };
}
