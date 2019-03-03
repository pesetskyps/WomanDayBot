import React from 'react';
import FetchData from './components/FetchData';

import 'materialize-css/dist/css/materialize.css';

export default () => (
  <div className="app">
    <h1>Orders list</h1>
    <FetchData
      className={'card-panel'} />
  </div>
);
