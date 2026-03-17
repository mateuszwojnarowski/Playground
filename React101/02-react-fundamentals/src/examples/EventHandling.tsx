/**
 * EventHandling – Responding to user interactions.
 *
 * Key concepts:
 *  - React events are camelCase (onClick, onChange, onSubmit).
 *  - Event handler functions receive a synthetic event object.
 *  - TypeScript provides specific event types for type-safety:
 *      MouseEvent, ChangeEvent, FormEvent, etc.
 */

import { useState } from 'react'

// 1. Click counter — demonstrates MouseEvent
function ClickCounter() {
  const [count, setCount] = useState(0)

  const handleClick = (_e: React.MouseEvent<HTMLButtonElement>) => {
    setCount((prev) => prev + 1)
  }

  return (
    <div className="card">
      <h3>Click Counter</h3>
      <p>Clicked {count} time{count !== 1 ? 's' : ''}</p>
      <button onClick={handleClick}>Click me</button>
    </div>
  )
}

// 2. Live input mirror — demonstrates ChangeEvent
function LiveInput() {
  const [text, setText] = useState('')

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setText(e.target.value)
  }

  return (
    <div className="card">
      <h3>Live Input</h3>
      <input
        type="text"
        value={text}
        onChange={handleChange}
        placeholder="Type something…"
      />
      <p>You typed: <strong>{text || '(nothing yet)'}</strong></p>
    </div>
  )
}

// 3. Form submission — demonstrates FormEvent
function SimpleForm() {
  const [name, setName] = useState('')
  const [submitted, setSubmitted] = useState('')

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault() // prevent page reload
    setSubmitted(name)
    setName('')
  }

  return (
    <div className="card">
      <h3>Form Submission</h3>
      <form onSubmit={handleSubmit}>
        <input
          type="text"
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="Enter your name"
        />
        <button type="submit" style={{ marginLeft: 8 }}>
          Submit
        </button>
      </form>
      {submitted && <p>Hello, <strong>{submitted}</strong>!</p>}
    </div>
  )
}

export function EventHandling() {
  return (
    <div className="example-container">
      <h2>Event Handling</h2>
      <ClickCounter />
      <LiveInput />
      <SimpleForm />
    </div>
  )
}
