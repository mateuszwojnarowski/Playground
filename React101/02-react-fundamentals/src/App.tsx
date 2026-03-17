import { NavLink, Route, Routes } from 'react-router-dom'
import { HelloWorld } from './examples/HelloWorld'
import { PropsExample } from './examples/PropsExample'
import { EventHandling } from './examples/EventHandling'
import { StateExample } from './examples/StateExample'
import { EffectExample } from './examples/EffectExample'
import { ListsAndConditionals } from './examples/ListsAndConditionals'
import { Counter } from './exercises/Exercise01_Counter'
import { TodoList } from './exercises/Exercise02_TodoList'
import { UserSearch } from './exercises/Exercise03_UserSearch'
import './App.css'

const examples = [
  { path: '/examples/hello-world', label: 'Hello World', element: <HelloWorld /> },
  { path: '/examples/props', label: 'Props', element: <PropsExample /> },
  { path: '/examples/events', label: 'Event Handling', element: <EventHandling /> },
  { path: '/examples/state', label: 'State (useState)', element: <StateExample /> },
  { path: '/examples/effects', label: 'Effects (useEffect)', element: <EffectExample /> },
  { path: '/examples/lists', label: 'Lists & Conditionals', element: <ListsAndConditionals /> },
]

const exercises = [
  { path: '/exercises/counter', label: 'Exercise 01: Counter', element: <Counter /> },
  { path: '/exercises/todo-list', label: 'Exercise 02: Todo List', element: <TodoList /> },
  { path: '/exercises/user-search', label: 'Exercise 03: User Search', element: <UserSearch /> },
]

function Home() {
  return (
    <div className="home">
      <h1>Section 02 – React Fundamentals</h1>
      <p>Select an example or exercise from the sidebar to get started.</p>

      <section>
        <h2>📚 Examples</h2>
        <ul>
          {examples.map((ex) => (
            <li key={ex.path}>
              <NavLink to={ex.path}>{ex.label}</NavLink>
            </li>
          ))}
        </ul>
      </section>

      <section>
        <h2>✏️ Exercises</h2>
        <ul>
          {exercises.map((ex) => (
            <li key={ex.path}>
              <NavLink to={ex.path}>{ex.label}</NavLink>
            </li>
          ))}
        </ul>
      </section>
    </div>
  )
}

export default function App() {
  return (
    <div className="app-layout">
      <nav className="sidebar">
        <h2>
          <NavLink to="/" style={{ textDecoration: 'none', color: 'inherit' }}>
            ⚛️ React 101
          </NavLink>
        </h2>

        <h3>Examples</h3>
        <ul>
          {examples.map((ex) => (
            <li key={ex.path}>
              <NavLink to={ex.path}>{ex.label}</NavLink>
            </li>
          ))}
        </ul>

        <h3>Exercises</h3>
        <ul>
          {exercises.map((ex) => (
            <li key={ex.path}>
              <NavLink to={ex.path}>{ex.label}</NavLink>
            </li>
          ))}
        </ul>
      </nav>

      <main className="main-content">
        <Routes>
          <Route path="/" element={<Home />} />
          {examples.map((ex) => (
            <Route key={ex.path} path={ex.path} element={ex.element} />
          ))}
          {exercises.map((ex) => (
            <Route key={ex.path} path={ex.path} element={ex.element} />
          ))}
        </Routes>
      </main>
    </div>
  )
}
