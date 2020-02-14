import React, { Component } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';

interface State {
  collapsed: boolean;
}

function NavBar() {
  let location = useLocation();

  return (
    <div className='d-flex justify-content-between mb-3'>
      {location.pathname != '/' && (
        <Link className="back-link" to='/'>
          <FontAwesomeIcon icon='arrow-left' size='xs' /> Back to Faculty List
        </Link>
      )}
      <span className='discreet ml-auto'>
        <span>Calvin Doval | CAES</span> • Sign out
      </span>
    </div>
  );
}

export class NavMenu extends Component<any, State> {
  static displayName = NavMenu.name;

  constructor(props: any) {
    super(props);

    this.toggleNavbar = this.toggleNavbar.bind(this);
    this.state = {
      collapsed: true
    };
  }

  toggleNavbar() {
    this.setState({
      collapsed: !this.state.collapsed
    });
  }

  render() {
    return (
      <header className='container'>
        <div className="logo-container">
          <img src='/media/fleece.svg' />
          <h3 className="mt-5 text-center">Faculty list encompassing everyone current &amp; Emeriti</h3>
        </div>
        
        <NavBar />
      </header>
    );
  }
}
