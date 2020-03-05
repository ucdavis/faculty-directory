import 'bootstrap/dist/css/bootstrap.css';
import React from 'react';
import ReactDOM from 'react-dom';
import { BrowserRouter } from 'react-router-dom';
import LogRocket from 'logrocket';

import App from './App';
import { unregister } from './registerServiceWorker';

import { library } from '@fortawesome/fontawesome-svg-core'
import { faArrowLeft, faTimes, faPlus, fas } from '@fortawesome/free-solid-svg-icons';

LogRocket.init('hlx0h5/facultydirectory');

library.add(faArrowLeft, faTimes, faPlus, fas)

const baseUrl = document.getElementsByTagName('base')[0].getAttribute('href') || undefined;
const rootElement = document.getElementById('root');

ReactDOM.render(
  <BrowserRouter basename={baseUrl}>
    <App />
  </BrowserRouter>,
  rootElement);

unregister(); // remove service workers
