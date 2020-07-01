import React from 'react';
import { Jumbotron } from 'reactstrap';

export const Error403 = () => {
  return (
    <div>
      <Jumbotron className='text-center'>
        <h1 className='display-1'>403</h1>
        <h1 className='display-3'>Access Denied/Forbidden</h1>

        <p className='display-5'>
          The page or resource you were trying to access is forbidden to you
        </p>
      </Jumbotron>
    </div>
  );
};
