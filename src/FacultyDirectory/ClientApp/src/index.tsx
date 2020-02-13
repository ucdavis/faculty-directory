import 'bootstrap/dist/css/bootstrap.css';
import React from 'react';
import ReactDOM from 'react-dom';
import { BrowserRouter } from 'react-router-dom';
import App from './App';
// import registerServiceWorker from './registerServiceWorker';
import { unregister } from './registerServiceWorker';

import { library } from '@fortawesome/fontawesome-svg-core'
import { faArrowLeft, faTimes, faPlus, fas } from '@fortawesome/free-solid-svg-icons';

library.add(faArrowLeft, faTimes, faPlus, fas)

const baseUrl = document.getElementsByTagName('base')[0].getAttribute('href') || undefined;
const rootElement = document.getElementById('root');

ReactDOM.render(
  <BrowserRouter basename={baseUrl}>
    <App />
  </BrowserRouter>,
  rootElement);

// registerServiceWorker();
unregister(); // remove service workers
