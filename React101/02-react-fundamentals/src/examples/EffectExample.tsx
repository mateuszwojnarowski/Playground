/**
 * EffectExample – Side effects with useEffect.
 *
 * Key concepts:
 *  - useEffect runs *after* the component renders.
 *  - The dependency array controls when the effect re-runs.
 *  - Return a cleanup function to tear down subscriptions / timers.
 *  - An empty dependency array [] means "run once on mount".
 */

import { useEffect, useState } from 'react'

// 1. Document title updater — runs on every count change
function DocumentTitle() {
  const [count, setCount] = useState(0)

  useEffect(() => {
    document.title = `Count: ${count}`
    // No cleanup needed for document.title
    return () => {
      document.title = 'React Fundamentals'
    }
  }, [count])

  return (
    <div className="card">
      <h3>Document Title Updater</h3>
      <p>Count: {count} (check the browser tab title!)</p>
      <button onClick={() => setCount((c) => c + 1)}>Increment</button>
    </div>
  )
}

// 2. Timer — demonstrates cleanup
function Timer() {
  const [seconds, setSeconds] = useState(0)
  const [running, setRunning] = useState(false)

  useEffect(() => {
    if (!running) return

    const id = setInterval(() => {
      setSeconds((s) => s + 1)
    }, 1000)

    // Cleanup: clear the interval when the component unmounts
    // or when `running` changes.
    return () => clearInterval(id)
  }, [running])

  return (
    <div className="card">
      <h3>Timer</h3>
      <p style={{ fontSize: '2rem', fontVariantNumeric: 'tabular-nums' }}>{seconds}s</p>
      <div style={{ display: 'flex', gap: 8 }}>
        <button onClick={() => setRunning((r) => !r)}>
          {running ? 'Pause' : 'Start'}
        </button>
        <button
          onClick={() => {
            setRunning(false)
            setSeconds(0)
          }}
        >
          Reset
        </button>
      </div>
    </div>
  )
}

// 3. Data fetching — fetch users from JSONPlaceholder

interface User {
  id: number
  name: string
  email: string
  company: { name: string }
}

function UserList() {
  const [users, setUsers] = useState<User[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false

    const fetchUsers = async () => {
      try {
        const res = await fetch('https://jsonplaceholder.typicode.com/users')
        if (!res.ok) throw new Error(`HTTP ${res.status}`)
        const data: User[] = await res.json()
        if (!cancelled) {
          setUsers(data)
          setLoading(false)
        }
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : 'Unknown error')
          setLoading(false)
        }
      }
    }

    fetchUsers()

    // Cleanup: if the component unmounts before the fetch completes,
    // we set `cancelled = true` so we don't update state.
    return () => {
      cancelled = true
    }
  }, [])

  if (loading) return <p>Loading users…</p>
  if (error) return <p style={{ color: 'red' }}>Error: {error}</p>

  return (
    <div className="card">
      <h3>Fetched Users (JSONPlaceholder)</h3>
      <ul>
        {users.map((user) => (
          <li key={user.id}>
            <strong>{user.name}</strong> — {user.email} ({user.company.name})
          </li>
        ))}
      </ul>
    </div>
  )
}

export function EffectExample() {
  return (
    <div className="example-container">
      <h2>Effects (useEffect)</h2>
      <DocumentTitle />
      <Timer />
      <UserList />
    </div>
  )
}
