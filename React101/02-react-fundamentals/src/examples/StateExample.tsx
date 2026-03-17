/**
 * StateExample – Managing component state with useState.
 *
 * Key concepts:
 *  - useState returns a [value, setter] tuple.
 *  - State updates trigger a re-render.
 *  - Use the callback form of the setter when the new value depends
 *    on the previous value: setState(prev => prev + 1).
 *  - Each piece of state should be as simple as possible.
 */

import { useState } from 'react'

// 1. Counter with increment / decrement / reset
function Counter() {
  const [count, setCount] = useState(0)

  return (
    <div className="card">
      <h3>Counter</h3>
      <p style={{ fontSize: '2rem', margin: '0.5rem 0' }}>{count}</p>
      <div style={{ display: 'flex', gap: 8 }}>
        <button onClick={() => setCount((c) => c - 1)}>−</button>
        <button onClick={() => setCount(0)}>Reset</button>
        <button onClick={() => setCount((c) => c + 1)}>+</button>
      </div>
    </div>
  )
}

// 2. Toggle switch — boolean state
function ToggleSwitch() {
  const [isOn, setIsOn] = useState(false)

  return (
    <div className="card">
      <h3>Toggle Switch</h3>
      <button
        onClick={() => setIsOn((prev) => !prev)}
        style={{
          padding: '8px 24px',
          background: isOn ? '#22c55e' : '#94a3b8',
          color: '#fff',
          border: 'none',
          borderRadius: 20,
          cursor: 'pointer',
        }}
      >
        {isOn ? 'ON' : 'OFF'}
      </button>
    </div>
  )
}

// 3. Multi-field form
function ProfileForm() {
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [bio, setBio] = useState('')

  return (
    <div className="card">
      <h3>Profile Form</h3>
      <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
        <input placeholder="Name" value={name} onChange={(e) => setName(e.target.value)} />
        <input placeholder="Email" value={email} onChange={(e) => setEmail(e.target.value)} />
        <textarea placeholder="Bio" value={bio} onChange={(e) => setBio(e.target.value)} rows={3} />
      </div>
      <h4 style={{ marginTop: 12 }}>Preview</h4>
      <p><strong>{name || '—'}</strong></p>
      <p>{email || '—'}</p>
      <p style={{ fontStyle: 'italic' }}>{bio || '—'}</p>
    </div>
  )
}

// 4. Dynamic list — add / remove items
function DynamicList() {
  const [items, setItems] = useState<string[]>(['Learn React', 'Build a project'])
  const [newItem, setNewItem] = useState('')

  const addItem = () => {
    const trimmed = newItem.trim()
    if (!trimmed) return
    setItems((prev) => [...prev, trimmed])
    setNewItem('')
  }

  const removeItem = (index: number) => {
    setItems((prev) => prev.filter((_, i) => i !== index))
  }

  return (
    <div className="card">
      <h3>Dynamic List</h3>
      <div style={{ display: 'flex', gap: 8 }}>
        <input
          value={newItem}
          onChange={(e) => setNewItem(e.target.value)}
          placeholder="Add an item…"
          onKeyDown={(e) => e.key === 'Enter' && addItem()}
        />
        <button onClick={addItem}>Add</button>
      </div>
      <ul>
        {items.map((item, index) => (
          <li key={index} style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <span>{item}</span>
            <button onClick={() => removeItem(index)} style={{ marginLeft: 8, fontSize: '0.8rem' }}>
              ✕
            </button>
          </li>
        ))}
      </ul>
      {items.length === 0 && <p style={{ color: '#94a3b8' }}>No items yet.</p>}
    </div>
  )
}

export function StateExample() {
  return (
    <div className="example-container">
      <h2>State (useState)</h2>
      <Counter />
      <ToggleSwitch />
      <ProfileForm />
      <DynamicList />
    </div>
  )
}
