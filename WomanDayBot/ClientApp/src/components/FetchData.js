import React, { Component, Fragment } from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';

import { actionCreators } from '../redux/reducer/index';

class FetchData extends Component {
  componentWillMount() {
    this.props.requestOrders();
  }

  componentWillReceiveProps(){
    this.props.requestOrders();
  }

  handleClick = (event, orderId, isComplete) => {
    this.props.updateOrder(orderId, isComplete);
  }

  template = (orders) => {
    return <table className='responsive-table'>
      <thead>
        <tr>
          <th>Order type</th>
          <th>Client name</th>
          <th>Room</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        {orders.map(order =>
          <tr className='asd' key={order.orderId}>
            <td>{order.orderType}</td>
            <td>{order.userData.name}</td>
            <td>{order.userData.room}</td>
            <td>
              <button
                className="btn waves-effect waves-light"
                type="button"
                name="action"
                onClick={(event) => this.handleClick(event, order.orderId, !order.isComplete)}>
                {order.isComplete ? "Incomplete" : "Complete"}
              </button>
            </td>
          </tr>
        )}
      </tbody>
    </table>
  }

  render() {
    const { orders, className } = this.props;
    const ordersList = orders.length ? this.template(orders) : <div className={className}>
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
