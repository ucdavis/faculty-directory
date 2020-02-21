import React from 'react';

export const Loading = () => {
    return (
      <div className='loading-scene'>
        <div className='loading-text'>LOADING...</div>
        <div className='sheep'>
          <span className='sheep-top'>
            <div className='sheep-body'></div>
            <div className='sheep-head'>
              <div className='sheep-eye sheep-one'></div>
              <div className='sheep-eye sheep-two'></div>
              <div className='sheep-ear sheep-one'></div>
              <div className='sheep-ear sheep-two'></div>
            </div>
          </span>
          <div className='sheep-legs'>
            <div className='sheep-leg'></div>
            <div className='sheep-leg'></div>
            <div className='sheep-leg'></div>
            <div className='sheep-leg'></div>
          </div>
        </div>
      </div>
    );
}