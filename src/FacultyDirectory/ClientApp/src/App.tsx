import React from 'react';
import { Route } from 'react-router';
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

const App = () => {
  const { isLoading } = useEnsureAuthenticated();

  if (isLoading) {
    return <Loading text='LOADING...'></Loading>;
  }

  return (
    <Layout>
      <Route exact path='/' component={Landing} />
      <Route path='/people/:id' component={Person} />
      <Route path='/pronunciation/:id' component={Pronunciation} />
      <Route exact path='/people' component={People} />
      <Route exact path='/fleece' component={Loading} />
      <Route exact path='/users' component={Users} />
      <Route exact path='/error403' component={Error403} />
    </Layout>
  );
};

export default App;
