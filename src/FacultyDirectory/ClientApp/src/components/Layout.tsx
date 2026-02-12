import { Outlet } from 'react-router-dom';
import { NavMenu } from './NavMenu';
import logoUrl from '/media/caes-logo.svg';

export const Layout = () => {
  return (
    <div>
      <div className='caes-wrapper'>
        <a href='https://caes.ucdavis.edu' target='_blank' rel='noreferrer'>
          <img src={logoUrl} alt='UCDAVIS CAES wordmark' />
        </a>
      </div>
      <NavMenu />
      <div className='container'>
        <Outlet />
      </div>
    </div>
  );
};
