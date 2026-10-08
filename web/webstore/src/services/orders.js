// Chưa có backend: giả lập tạo đơn. Thay bằng lời gọi API thật (vd. POST /api/orders) khi có server.
export async function createOrder(order) {
  return { id: 'BH' + Math.floor(100000 + Math.random() * 900000), ...order }
}
