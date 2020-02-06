import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { People } from './components/People';

import './custom.scss'
import { Person } from './components/Person';

export default class App extends Component {
  static displayName = App.name;

  render () {
    return (
      <Layout>
        <Route exact path='/' component={People} />
        <Route path='/people/:id' component={Person} />
        <Route exact path='/people' component={People} />
      </Layout>
    );
  }
}
