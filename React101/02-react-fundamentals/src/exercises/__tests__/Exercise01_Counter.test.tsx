import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Counter } from '../Exercise01_Counter'

describe('Counter', () => {
  it('renders initial count of 0', () => {
    render(<Counter />)
    expect(screen.getByText(/0/)).toBeInTheDocument()
  })

  it('increments count when + is clicked', async () => {
    const user = userEvent.setup()
    render(<Counter />)

    await user.click(screen.getByRole('button', { name: '+' }))
    expect(screen.getByText(/1/)).toBeInTheDocument()
  })

  it('decrements count when - is clicked', async () => {
    const user = userEvent.setup()
    render(<Counter />)

    // First increment so we can decrement
    await user.click(screen.getByRole('button', { name: '+' }))
    await user.click(screen.getByRole('button', { name: '+' }))
    await user.click(screen.getByRole('button', { name: /−|-/ }))
    expect(screen.getByText(/1/)).toBeInTheDocument()
  })

  it('does not go below 0', async () => {
    const user = userEvent.setup()
    render(<Counter />)

    // Try to decrement from 0
    await user.click(screen.getByRole('button', { name: /−|-/ }))
    expect(screen.getByText(/0/)).toBeInTheDocument()
    // Should not display negative numbers
    expect(screen.queryByText(/-1/)).not.toBeInTheDocument()
  })

  it('resets count to 0', async () => {
    const user = userEvent.setup()
    render(<Counter />)

    await user.click(screen.getByRole('button', { name: '+' }))
    await user.click(screen.getByRole('button', { name: '+' }))
    await user.click(screen.getByRole('button', { name: /reset/i }))
    expect(screen.getByText(/0/)).toBeInTheDocument()
  })

  it('displays "Count is zero" when count is 0', () => {
    render(<Counter />)
    expect(screen.getByText(/count is zero/i)).toBeInTheDocument()
  })
})
