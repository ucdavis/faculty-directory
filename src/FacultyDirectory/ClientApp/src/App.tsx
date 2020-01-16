import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { People } from './components/People';

import './custom.css'
import { Person } from './components/Person';

export default class App extends Component {
  static displayName = App.name;

  render () {
    return (
      <Layout>
        <Route exact path='/' component={Home} />
        <Route path='/people/:id' component={Person} />
        <Route exact path='/people' component={People} />
      </Layout>
    );
  }
}
