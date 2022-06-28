import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { People } from './components/People';

import './sass/custom.scss';
import { Person } from './components/Person';
import { Loading } from './components/Loading';
import { Users } from './components/UserTable/Users';
import { Error403 } from './components/Error403';
import { Pronunciation } from './components/Pronunciation';

export default class App extends Component {
  static displayName = App.name;

  render() {
    return (
      <Layout>
        <Route exact path='/' component={People} />
        <Route path='/people/:id' component={Person} />
        <Route path='/pronunciation/:id' component={Pronunciation} />
        <Route exact path='/people' component={People} />
        <Route exact path='/fleece' component={Loading} />
        <Route exact path='/users' component={Users} />
        <Route exact path='/error403' component={Error403} />
      </Layout>
    );
  }
}
