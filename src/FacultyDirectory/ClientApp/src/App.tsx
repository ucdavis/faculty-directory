import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import { Layout } from './components/Layout';
import { People } from './components/People';

import './sass/custom.scss';
import { Person } from './components/Person';
import { Loading } from './components/Loading';
import { Users } from './components/UserTable/Users';
import { Error403 } from './components/Error403';
import { Pronunciation } from './components/Pronunciation';
import { Landing } from './components/Landing';
import { useEnsureAuthenticated } from './util/useEnsureAuthenticated';

const router = createBrowserRouter([
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
]);

const App = () => {
  const { isLoading } = useEnsureAuthenticated();

  if (isLoading) {
    return <Loading text='LOADING...' />;
  }

  return <RouterProvider router={router} />;
};

export default App;
