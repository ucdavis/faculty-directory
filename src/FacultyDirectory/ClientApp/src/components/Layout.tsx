import { Outlet } from 'react-router-dom';
import { NavMenu } from './NavMenu';

export const Layout = () => {
  return (
    <div>
      <div className='caes-wrapper'>
        <a href='https://caes.ucdavis.edu' target='_blank' rel='noreferrer'>
          <img src='/media/caes-logo.svg' alt='UCDAVIS CAES wordmark' />
        </a>
      </div>
      <NavMenu />
      <div className='container'>
        <Outlet />
      </div>
    </div>
  );
};
