import { RECEIVE_ORDERS, REQUEST_ORDERS, UPDATE_ORDER } from '../constants/index'

const initialState = { orders: [] };

export const actionCreators = {
  requestOrders: () => async (dispatch, getState) => {

    dispatch({ type: REQUEST_ORDERS });

    const url = `api/Orders/GetOrdersAsync`;
    const response = await fetch(url);
    const orders = await response.json();

    dispatch({ type: RECEIVE_ORDERS, orders });
  },

  updateOrder: (orderId, isComplete) => async (dispatch, getState) => {
    // dispatch({ type: REQUEST_ORDERS });
    let state = getState();
    let { orders } = state.orders;
    const order = orders.filter(obj => obj.orderId === orderId)[0];

    order.isComplete = isComplete;
    const url = `api/Orders/UpdateOrderAsync`;
    const response = await fetch(url, {
      method: 'PUT',
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(order)
    });

    dispatch({ type: RECEIVE_ORDERS, orders });
  }
};

export const reducer = (state, action) => {
  state = state || initialState;

  if (action.type === REQUEST_ORDERS) {
    return {
      ...state
    };
  }

  if (action.type === RECEIVE_ORDERS) {
    return {
      ...state,
      orders: action.orders
    };
  }

  return state;
};
