import React, { Component, Fragment } from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';

import { actionCreators } from '../redux/reducer/index';
import './index.css'

class FetchData extends Component {
  componentWillMount() {
    this.props.requestOrders();
  }

  componentWillReceiveProps() {
    this.props.requestOrders();
  }

  handleClick = (event, orderId, isComplete) => {
    this.props.updateOrder(orderId, isComplete);
  }

  formatTime = (requestTime) => {
    const date = new Date(requestTime);
    return date.toLocaleTimeString();
  }

  template = (orders) => {
    return <table>
      <thead>
        <tr>
          <th>#</th>
          <th>Order type</th>
          <th>Client name</th>
          <th>Room</th>
          <th>Comment</th>
          <th>Time</th>
          <th>Action button</th>
        </tr>
      </thead>
      <tbody>
        {orders.map((order, index) =>
          <tr className='asd' key={order.orderId}>
            <td>{index}</td>
            <td>{order.orderType}</td>
            <td>{order.userData.name}</td>
            <td>{order.userData.room}</td>
            <td>{order.comment}</td>
            <td>{this.formatTime(order.requestTime)}</td>
            <td>
              <button
                className={order.isComplete ? "btn waves-effect waves-light red" : "btn waves-effect waves-light"}
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

  layout = (orders) => {
    return <div className="row" id='row'>
      <div className="col s6" id="incompleted-orders"><span>Incompleted</span>{this.template(orders.filter(x => !x.isComplete))}</div>
      <div className="col s6" id="incompleted-orders"><span>Completed</span>{this.template(orders.filter(x => x.isComplete))}</div>
    </div>
  }

  render() {
    const { orders, className } = this.props;
    const ordersList = orders.length ? this.layout(orders) : <div className={className}>
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
