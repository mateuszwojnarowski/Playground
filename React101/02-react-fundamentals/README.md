# Section 02 – React Fundamentals

A hands-on project for learning core React concepts using **React 19 + TypeScript + Vite**.

---

## What You'll Learn

| Topic                  | Example file               |
| ---------------------- | -------------------------- |
| JSX & Component basics | `HelloWorld.tsx`           |
| Props & TypeScript     | `PropsExample.tsx`         |
| Event Handling         | `EventHandling.tsx`        |
| State (`useState`)     | `StateExample.tsx`         |
| Effects (`useEffect`)  | `EffectExample.tsx`        |
| Lists & Conditionals   | `ListsAndConditionals.tsx` |

---

## Getting Started

```bash
# Install dependencies
npm install

# Start the dev server
npm run dev

# Run tests (exercises)
npm test

# Run tests in watch mode
npm run test:watch
```

Open <http://localhost:5173> and use the sidebar to navigate between examples and exercises.

---

## Topics

### 1. Hello World (`src/examples/HelloWorld.tsx`)

The simplest React component — a function that returns JSX.

```tsx
export function HelloWorld() {
  return <h1>Hello, World!</h1>
}
```

**Key points:** JSX compiles to `React.createElement()` calls. Use `className` instead of `class`, and wrap expressions in `{}`.

### 2. Props (`src/examples/PropsExample.tsx`)

Props are the inputs to a component — like function parameters.

```tsx
interface UserCardProps {
  name: string
  email: string
  avatar?: string // optional
}

function UserCard({ name, email, avatar }: UserCardProps) {
  return (
    <div>
      <h3>{name}</h3>
      <p>{email}</p>
    </div>
  )
}
```

### 3. Event Handling (`src/examples/EventHandling.tsx`)

React events are **camelCase** (`onClick`, `onChange`). TypeScript gives you typed events:

```tsx
const handleClick = (e: React.MouseEvent<HTMLButtonElement>) => { ... }
const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => { ... }
const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
  e.preventDefault()
  ...
}
```

### 4. State — `useState` (`src/examples/StateExample.tsx`)

State lets a component "remember" values between renders.

```tsx
const [count, setCount] = useState(0)

// Use the callback form when new state depends on previous state
setCount(prev => prev + 1)
```

### 5. Effects — `useEffect` (`src/examples/EffectExample.tsx`)

Side effects (data fetching, timers, subscriptions) go in `useEffect`.

```tsx
useEffect(() => {
  document.title = `Count: ${count}`
  return () => { /* cleanup */ }
}, [count]) // re-run when count changes
```

### 6. Lists & Conditionals (`src/examples/ListsAndConditionals.tsx`)

```tsx
// Rendering a list — always provide a key
{items.map(item => <li key={item.id}>{item.name}</li>)}

// Conditional rendering
{isLoggedIn && <p>Welcome back!</p>}
{count > 0 ? <Badge count={count} /> : null}
```

---

## Exercises

Each exercise is a **placeholder component** — the tests describe the expected behaviour. Implement the component to make the tests pass.

### Exercise 01: Counter (`src/exercises/Exercise01_Counter.tsx`)

Build a counter with increment (+), decrement (−), and reset buttons.

**Acceptance criteria:**
- Displays the current count (starts at 0)
- `+` increments, `−` decrements, *Reset* sets to 0
- Count never goes below 0
- Shows "Count is zero" when the count is 0

Run: `npm test -- Exercise01`

### Exercise 02: Todo List (`src/exercises/Exercise02_TodoList.tsx`)

Build a classic todo list.

**Acceptance criteria:**
- Input + *Add* button to create todos
- Each todo has a checkbox (complete) and *Delete* button
- Completed todos show strikethrough text
- Displays count of remaining (incomplete) todos

Run: `npm test -- Exercise02`

### Exercise 03: User Search (`src/exercises/Exercise03_UserSearch.tsx`)

Build a component that fetches and searches users.

**Acceptance criteria:**
- Fetches users from `https://jsonplaceholder.typicode.com/users` on mount
- Shows a loading message while fetching
- Shows an error message if the fetch fails
- Search input filters users by name
- User cards display name, email, and company name
- Shows "No users found" when the search matches nothing

Run: `npm test -- Exercise03`

---

## Key Concepts – Quick Reference

### `useState`

```tsx
const [value, setValue] = useState(initialValue)
```

- Returns `[currentValue, setterFunction]`.
- Calling the setter triggers a re-render.
- Use the callback form (`setValue(prev => ...)`) when the new value depends on the old.

### `useEffect`

```tsx
useEffect(() => {
  // effect
  return () => { /* cleanup */ }
}, [dependencies])
```

| Dependency array | When does the effect run?         |
| ---------------- | --------------------------------- |
| `[]`             | Once, on mount                    |
| `[a, b]`        | When `a` or `b` change           |
| *(omitted)*      | After every render                |

### JSX Rules

1. Return a **single root element** (or use `<>...</>` fragments).
2. **Close all tags** — including self-closing ones (`<img />`, `<br />`).
3. Use **camelCase** for HTML attributes (`className`, `htmlFor`, `onClick`).
4. Embed JavaScript with **curly braces**: `{expression}`.

---

## Project Structure

```
02-react-fundamentals/
├── src/
│   ├── examples/         # Fully-working example components
│   │   ├── HelloWorld.tsx
│   │   ├── PropsExample.tsx
│   │   ├── EventHandling.tsx
│   │   ├── StateExample.tsx
│   │   ├── EffectExample.tsx
│   │   └── ListsAndConditionals.tsx
│   ├── exercises/        # Placeholder components (your turn!)
│   │   ├── Exercise01_Counter.tsx
│   │   ├── Exercise02_TodoList.tsx
│   │   ├── Exercise03_UserSearch.tsx
│   │   └── __tests__/    # Tests for the exercises
│   ├── test/
│   │   └── setup.ts      # Vitest setup (jest-dom matchers)
│   ├── App.tsx            # Routing & layout
│   ├── App.css
│   ├── index.css
│   └── main.tsx           # Entry point
├── vite.config.ts
├── tsconfig.*.json
└── package.json
```
