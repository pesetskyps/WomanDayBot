import React from 'react';
import ReactDOM from 'react-dom';
import { Provider } from 'react-redux';
import registerServiceWorker from './registerServiceWorker';

import App from './App';
import configureStore from '../src/redux/store/index';
import './index.css';

const initialState = window.initialReduxState;
const store = configureStore(initialState);

const rootElement = document.getElementById('root');

ReactDOM.render(
  <Provider store={store}>
      <App />
  </Provider>,
  rootElement);

registerServiceWorker();
