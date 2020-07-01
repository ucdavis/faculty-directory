import React, { Component, useEffect, useState } from 'react';
import { useHistory } from 'react-router-dom';
import { Link, useLocation } from 'react-router-dom';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';

interface State {
  collapsed: boolean;
}

function NavBar() {
  let location = useLocation();
  let history = useHistory();
  let [name, setName] = useState('');

  useEffect(() => {
    const getName = async () => {
      const results = await fetch('api/users/name');
      const response = await results;

      if (response.status === 403) {
        history.push('/error403');
      } else {
        const userData = await results.json();
        setName(userData.name);
      }
    };
    getName();
  }, []);

  return (
    <div className='text-center'>
      {location.pathname != '/' && (
        <p className='mb-2'>
          <Link className='back-link' to='/'>
            <FontAwesomeIcon icon='arrow-left' size='xs' /> Back to Faculty List
          </Link>
        </p>
      )}
      <p className='mb-1 discreet ml-auto'>
        <span>{name} | CAES</span> • Sign out
      </p>
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
      <header>
        <div className='logo-container text-center'>
          <Link to='/fleece'>
            <img src='/media/sheep.svg' />
          </Link>
          <h1 className='mt-4'>FLEECE</h1>
          <p className='lede'>
            Faculty List Encompassing Everyone Current and Emeriti
          </p>
        </div>

        <NavBar />
      </header>
    );
  }
}
