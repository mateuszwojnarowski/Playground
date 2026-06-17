/**
 * PropsExample – Passing data to components via props.
 *
 * Key concepts:
 *  - Props are the inputs to a component (like function arguments).
 *  - TypeScript interfaces define the shape of props.
 *  - Props are read-only — a component must never modify its own props.
 *  - The special `children` prop lets you nest content inside a component.
 */

// Define the shape of props with a TypeScript interface.
interface UserCardProps {
  name: string
  email: string
  avatar?: string // optional prop
}

// Component that receives typed props.
function UserCard({ name, email, avatar }: UserCardProps) {
  return (
    <div className="card">
      <img
        src={avatar ?? `https://ui-avatars.com/api/?name=${encodeURIComponent(name)}&background=random`}
        alt={`${name}'s avatar`}
        width={64}
        height={64}
        style={{ borderRadius: '50%' }}
      />
      <div>
        <h3>{name}</h3>
        <p>{email}</p>
      </div>
    </div>
  )
}

// Demonstrates the `children` prop for component composition.
interface GreetingProps {
  children: React.ReactNode
}

function Greeting({ children }: GreetingProps) {
  return (
    <div className="card" style={{ background: '#f0f9ff' }}>
      <p style={{ fontSize: '1.1rem' }}>👋 {children}</p>
    </div>
  )
}

// Sample data
const users: UserCardProps[] = [
  { name: 'Alice Johnson', email: 'alice@example.com' },
  { name: 'Bob Smith', email: 'bob@example.com' },
  {
    name: 'Carol Williams',
    email: 'carol@example.com',
    avatar: 'https://ui-avatars.com/api/?name=Carol+W&background=e0aaff',
  },
]

export function PropsExample() {
  return (
    <div className="example-container">
      <h2>Props Example</h2>

      <section>
        <h3>UserCard components with typed props</h3>
        {users.map((user) => (
          <UserCard key={user.email} {...user} />
        ))}
      </section>

      <section>
        <h3>Children prop</h3>
        <Greeting>
          Welcome to <strong>React Fundamentals</strong>!
        </Greeting>
        <Greeting>
          Props let you pass data <em>into</em> components.
        </Greeting>
      </section>
    </div>
  )
}
