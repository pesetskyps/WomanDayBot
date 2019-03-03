import React, { Component, Fragment } from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';

import { actionCreators } from '../redux/reducer/index';

class FetchData extends Component {
  componentWillMount() {
    this.props.requestOrders();
  }

  handleChange = (event, orderId) => {
    this.props.updateOrder(orderId, event.target.checked);
  }

  template = ({ orderId, orderType, isComplete }) => {
    return <div key={orderId} className={this.props.className}>
      <label>
        <input
          checked={isComplete}
          data-role='check'
          onChange={(event) => this.handleChange(event, orderId)}
          type='checkbox' />
        <span
          className={'blue-text text-darken-2'}>
          {orderType}
        </span>
      </label>
    </div>
  }

  render() {
    const { orders, className } = this.props;
    const ordersList = orders.length ? orders.map(x => {
      return this.template(x)
    }) : <div className={className}>
        <span className="blue-text text-darken-2">You don't have any orders.</span>
      </div>;
    return (
      <Fragment>
        {ordersList}
      </Fragment>
    );
  }
}

export default connect(
  state => state.orders,
  dispatch => bindActionCreators(actionCreators, dispatch)
)(FetchData);
