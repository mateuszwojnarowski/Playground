import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { UserSearch } from '../Exercise03_UserSearch'

const mockUsers = [
  { id: 1, name: 'Leanne Graham', email: 'leanne@example.com', company: { name: 'Romaguera' } },
  { id: 2, name: 'Ervin Howell', email: 'ervin@example.com', company: { name: 'Deckow' } },
  { id: 3, name: 'Clementine Bauch', email: 'clem@example.com', company: { name: 'Hoeger' } },
]

describe('UserSearch', () => {
  beforeEach(() => {
    vi.restoreAllMocks()
  })

  it('shows loading message while fetching', () => {
    vi.spyOn(globalThis, 'fetch').mockImplementation(
      () => new Promise(() => {}), // never resolves
    )

    render(<UserSearch />)
    expect(screen.getByText(/loading/i)).toBeInTheDocument()
  })

  it('renders users after successful fetch', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce({
      ok: true,
      json: async () => mockUsers,
    } as Response)

    render(<UserSearch />)

    await waitFor(() => {
      expect(screen.getByText('Leanne Graham')).toBeInTheDocument()
    })

    expect(screen.getByText('Ervin Howell')).toBeInTheDocument()
    expect(screen.getByText('Clementine Bauch')).toBeInTheDocument()
  })

  it('shows error message on fetch failure', async () => {
    vi.spyOn(globalThis, 'fetch').mockRejectedValueOnce(new Error('Network error'))

    render(<UserSearch />)

    await waitFor(() => {
      expect(screen.getByText(/error/i)).toBeInTheDocument()
    })
  })

  it('filters users by name', async () => {
    const user = userEvent.setup()

    vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce({
      ok: true,
      json: async () => mockUsers,
    } as Response)

    render(<UserSearch />)

    await waitFor(() => {
      expect(screen.getByText('Leanne Graham')).toBeInTheDocument()
    })

    const searchInput = screen.getByPlaceholderText(/search/i)
    await user.type(searchInput, 'Ervin')

    expect(screen.getByText('Ervin Howell')).toBeInTheDocument()
    expect(screen.queryByText('Leanne Graham')).not.toBeInTheDocument()
    expect(screen.queryByText('Clementine Bauch')).not.toBeInTheDocument()
  })

  it('shows "No users found" when search has no results', async () => {
    const user = userEvent.setup()

    vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce({
      ok: true,
      json: async () => mockUsers,
    } as Response)

    render(<UserSearch />)

    await waitFor(() => {
      expect(screen.getByText('Leanne Graham')).toBeInTheDocument()
    })

    const searchInput = screen.getByPlaceholderText(/search/i)
    await user.type(searchInput, 'zzzzz')

    expect(screen.getByText(/no users found/i)).toBeInTheDocument()
  })

  it('displays email and company name for each user', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce({
      ok: true,
      json: async () => mockUsers,
    } as Response)

    render(<UserSearch />)

    await waitFor(() => {
      expect(screen.getByText('Leanne Graham')).toBeInTheDocument()
    })

    expect(screen.getByText('leanne@example.com')).toBeInTheDocument()
    expect(screen.getByText('Romaguera')).toBeInTheDocument()
  })
})
