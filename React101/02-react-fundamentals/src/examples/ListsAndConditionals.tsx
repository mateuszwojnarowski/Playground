/**
 * ListsAndConditionals – Rendering lists and conditional content.
 *
 * Key concepts:
 *  - Use .map() to render arrays of elements.
 *  - Every list item needs a unique `key` prop.
 *  - Conditional rendering: &&, ternary, early return.
 */

import { useState } from 'react'

// Sample data
interface Fruit {
  id: number
  name: string
  emoji: string
  color: string
}

const fruits: Fruit[] = [
  { id: 1, name: 'Apple', emoji: '🍎', color: 'red' },
  { id: 2, name: 'Banana', emoji: '🍌', color: 'yellow' },
  { id: 3, name: 'Cherry', emoji: '🍒', color: 'red' },
  { id: 4, name: 'Grape', emoji: '🍇', color: 'purple' },
  { id: 5, name: 'Kiwi', emoji: '🥝', color: 'green' },
  { id: 6, name: 'Lemon', emoji: '🍋', color: 'yellow' },
  { id: 7, name: 'Mango', emoji: '🥭', color: 'orange' },
  { id: 8, name: 'Orange', emoji: '🍊', color: 'orange' },
]

// 1. Simple list with keys
function FruitList() {
  return (
    <div className="card">
      <h3>Fruit List (with keys)</h3>
      <ul>
        {fruits.map((fruit) => (
          <li key={fruit.id}>
            {fruit.emoji} {fruit.name}
          </li>
        ))}
      </ul>
    </div>
  )
}

// 2. Conditional rendering patterns
function ConditionalPatterns() {
  const [isLoggedIn, setIsLoggedIn] = useState(false)
  const [messageCount, setMessageCount] = useState(0)

  return (
    <div className="card">
      <h3>Conditional Rendering</h3>

      {/* Pattern 1: && (short-circuit) */}
      {messageCount > 0 && (
        <p className="badge">
          You have {messageCount} unread message{messageCount !== 1 ? 's' : ''}
        </p>
      )}

      {/* Pattern 2: Ternary */}
      <p>{isLoggedIn ? '✅ Logged in' : '❌ Not logged in'}</p>

      <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
        <button onClick={() => setIsLoggedIn((v) => !v)}>
          {isLoggedIn ? 'Log out' : 'Log in'}
        </button>
        <button onClick={() => setMessageCount((c) => c + 1)}>
          Add message
        </button>
        <button onClick={() => setMessageCount(0)}>Clear messages</button>
      </div>
    </div>
  )
}

// 3. Early return pattern
function UserStatus({ isOnline }: { isOnline: boolean }) {
  if (!isOnline) {
    return <span style={{ color: '#94a3b8' }}>⚫ Offline</span>
  }
  return <span style={{ color: '#22c55e' }}>🟢 Online</span>
}

// 4. Filterable list
function FilterableList() {
  const [query, setQuery] = useState('')

  const filtered = fruits.filter((f) =>
    f.name.toLowerCase().includes(query.toLowerCase()),
  )

  return (
    <div className="card">
      <h3>Filterable List</h3>
      <input
        type="text"
        value={query}
        onChange={(e) => setQuery(e.target.value)}
        placeholder="Search fruits…"
      />
      {filtered.length === 0 ? (
        <p style={{ color: '#94a3b8' }}>No fruits match "{query}"</p>
      ) : (
        <ul>
          {filtered.map((fruit) => (
            <li key={fruit.id}>
              {fruit.emoji} {fruit.name}{' '}
              <small style={{ color: '#94a3b8' }}>({fruit.color})</small>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}

// 5. Show / hide section
function CollapsibleSection() {
  const [isOpen, setIsOpen] = useState(false)

  return (
    <div className="card">
      <h3>
        Collapsible Section{' '}
        <button onClick={() => setIsOpen((v) => !v)} style={{ fontSize: '0.9rem' }}>
          {isOpen ? 'Hide ▲' : 'Show ▼'}
        </button>
      </h3>
      {isOpen && (
        <div>
          <p>This content is conditionally rendered.</p>
          <p>
            User status:{' '}
            <UserStatus isOnline={true} /> / <UserStatus isOnline={false} />
          </p>
        </div>
      )}
    </div>
  )
}

export function ListsAndConditionals() {
  return (
    <div className="example-container">
      <h2>Lists & Conditional Rendering</h2>
      <FruitList />
      <ConditionalPatterns />
      <FilterableList />
      <CollapsibleSection />
    </div>
  )
}
