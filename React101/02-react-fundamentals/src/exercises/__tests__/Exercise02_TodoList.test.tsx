import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { TodoList } from '../Exercise02_TodoList'

describe('TodoList', () => {
  it('renders the input and add button', () => {
    render(<TodoList />)
    expect(screen.getByPlaceholderText(/add/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /add/i })).toBeInTheDocument()
  })

  it('adds a new todo', async () => {
    const user = userEvent.setup()
    render(<TodoList />)

    const input = screen.getByPlaceholderText(/add/i)
    await user.type(input, 'Buy groceries')
    await user.click(screen.getByRole('button', { name: /add/i }))

    expect(screen.getByText('Buy groceries')).toBeInTheDocument()
  })

  it('clears the input after adding a todo', async () => {
    const user = userEvent.setup()
    render(<TodoList />)

    const input = screen.getByPlaceholderText(/add/i) as HTMLInputElement
    await user.type(input, 'Walk the dog')
    await user.click(screen.getByRole('button', { name: /add/i }))

    expect(input.value).toBe('')
  })

  it('marks a todo as complete with a checkbox', async () => {
    const user = userEvent.setup()
    render(<TodoList />)

    const input = screen.getByPlaceholderText(/add/i)
    await user.type(input, 'Read a book')
    await user.click(screen.getByRole('button', { name: /add/i }))

    const checkbox = screen.getByRole('checkbox')
    await user.click(checkbox)

    expect(checkbox).toBeChecked()
  })

  it('applies strikethrough style to completed todos', async () => {
    const user = userEvent.setup()
    render(<TodoList />)

    const input = screen.getByPlaceholderText(/add/i)
    await user.type(input, 'Clean house')
    await user.click(screen.getByRole('button', { name: /add/i }))

    const checkbox = screen.getByRole('checkbox')
    await user.click(checkbox)

    const todoText = screen.getByText('Clean house')
    expect(todoText).toHaveStyle('text-decoration: line-through')
  })

  it('deletes a todo', async () => {
    const user = userEvent.setup()
    render(<TodoList />)

    const input = screen.getByPlaceholderText(/add/i)
    await user.type(input, 'Delete me')
    await user.click(screen.getByRole('button', { name: /add/i }))

    expect(screen.getByText('Delete me')).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: /delete/i }))

    expect(screen.queryByText('Delete me')).not.toBeInTheDocument()
  })

  it('shows remaining (incomplete) todo count', async () => {
    const user = userEvent.setup()
    render(<TodoList />)

    const input = screen.getByPlaceholderText(/add/i)

    await user.type(input, 'Task 1')
    await user.click(screen.getByRole('button', { name: /add/i }))
    await user.type(input, 'Task 2')
    await user.click(screen.getByRole('button', { name: /add/i }))

    expect(screen.getByText(/2.*remaining/i)).toBeInTheDocument()

    // Complete one
    const checkboxes = screen.getAllByRole('checkbox')
    await user.click(checkboxes[0])

    expect(screen.getByText(/1.*remaining/i)).toBeInTheDocument()
  })
})
