import { describe, it, expect, beforeEach, vi } from 'vitest';
import { render } from '@testing-library/react';
import { createMemoryRouter, RouterProvider } from 'react-router-dom';
import { Layout } from './components/Layout';
import { Landing } from './components/Landing';
import { People } from './components/People';
import { Person } from './components/Person';
import { Pronunciation } from './components/Pronunciation';
import { Loading } from './components/Loading';
import { Users } from './components/UserTable/Users';
import { Error403 } from './components/Error403';

// Mock the useEnsureAuthenticated hook
vi.mock('./util/useEnsureAuthenticated', () => ({
  useEnsureAuthenticated: () => ({
    isLoading: false,
    userInfo: null,
  }),
}));

beforeEach(() => {
  // Mock fetch to return appropriate responses
  (window as any).fetch = vi.fn().mockImplementation((url: string) => {
    if (url === 'api/faculty/name') {
      return Promise.resolve({
        ok: true,
        status: 200,
        json: () => Promise.resolve({ name: 'Test User' }),
      });
    }
    return Promise.resolve({
      ok: true,
      status: 200,
      json: () => Promise.resolve([]),
    });
  }) as any;
});

describe('App', () => {
  it('renders without crashing', () => {
    const router = createMemoryRouter(
      [
        {
          path: '/',
          element: <Layout />,
          children: [
            {
              index: true,
              element: <Landing />,
            },
            {
              path: 'people/:id',
              element: <Person />,
            },
            {
              path: 'pronunciation/:id',
              element: <Pronunciation />,
            },
            {
              path: 'people',
              element: <People />,
            },
            {
              path: 'fleece',
              element: <Loading text='LOADING...' />,
            },
            {
              path: 'users',
              element: <Users />,
            },
            {
              path: 'error403',
              element: <Error403 />,
            },
          ],
        },
      ],
      {
        initialEntries: ['/'],
      }
    );

    const { container } = render(<RouterProvider router={router} />);
    expect(container).toBeTruthy();
  });
});
