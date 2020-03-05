import React from 'react';
import ReactDOM from 'react-dom';
import { MemoryRouter } from 'react-router-dom';
import App from './App';
import { act } from 'react-dom/test-utils';

declare var global: any;

beforeEach(function() {
  // mock fetch to return people list
  global.fetch = jest.fn().mockImplementation(() => {
    var p = new Promise((resolve, reject) => {
      resolve({
        ok: true,
        json: function() {
          return [];
        }
      });
    });

    return p;
  });
});

it('renders without crashing', async () => {
  // need to wrap in act(()=>{}) because default route sets state with useEffect
  await act(async () => {
    const div = document.createElement('div');
    ReactDOM.render(
      <MemoryRouter>
        <App />
      </MemoryRouter>,
      div
    );
    await new Promise(resolve => setTimeout(resolve, 1000));
  });
});
