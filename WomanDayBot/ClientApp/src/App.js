import React from 'react';
import FetchData from './components/FetchData';

import 'materialize-css/dist/css/materialize.css';

export default () => (
  <div className="app" id="app">
    <FetchData
      className={'card-panel'} />
  </div>
);
