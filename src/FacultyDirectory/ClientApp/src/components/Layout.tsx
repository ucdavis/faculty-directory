import React, { Component } from 'react';
import { Container } from 'reactstrap';
import { NavMenu } from './NavMenu';

export class Layout extends Component {
  static displayName = Layout.name;

  render() {
    return (
      <div>
        <div className='caes-wrapper'>
          <a href='https://caes.ucdavis.edu' target='_blank'>
            <img src='/media/caes-logo.svg' alt='UCDAVIS CAES wordmark' />
          </a>
        </div>
        <NavMenu />
        <Container>{this.props.children}</Container>
      </div>
    );
  }
}
