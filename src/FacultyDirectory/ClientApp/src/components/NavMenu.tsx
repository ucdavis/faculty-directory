import React, { Component } from 'react';
import './NavMenu.css';

interface State {
  collapsed: boolean;
}

export class NavMenu extends Component<any, State> {
  static displayName = NavMenu.name;

  constructor (props: any) {
    super(props);

    this.toggleNavbar = this.toggleNavbar.bind(this);
    this.state = {
      collapsed: true
    };
  }

  toggleNavbar () {
    this.setState({
      collapsed: !this.state.collapsed
    });
  }

  render () {
    return (
      <header className="container">
        <h1>Faculty list encompassing everyone current &amp; Emeriti</h1>
        <p className="discreet">
        <span>Calvin Doval | CAES</span> •
        Sign out
        </p>
        <a href="/">Home Table</a>
      </header>
    );
  }
}
