// Trang chủ "A Day with Bosch" — phần trình bày nằm ở frontend: ảnh hero, bảng màu nền theo giờ, ảnh các ô danh mục.
// Nội dung từng khoảnh khắc (giờ, lời dẫn, ảnh, sản phẩm) nằm trong DB (bảng HomeMoments), lấy qua GET /api/home.
// Ảnh nằm trong public/images/.

export const DAWN = {
  time: '06:32',
  img: '/images/scene-0732-bep.jpg',
  alt: 'Căn bếp với tủ lạnh, máy rửa bát và lò nướng Bosch',
}

// Dải "Ưu đãi chỉ trong hôm nay" nằm ngay sau khoảnh khắc này
export const FLASH_AFTER = 'bua-trua'

// Màu nền phần "Ngày" theo giờ; trang chủ pha dần giữa hai màu liền nhau khi cuộn
export const TONES = {
  dawn: 'oklch(0.95 0.022 75)',
  morning: 'oklch(0.965 0.032 92)',
  day: 'oklch(0.965 0.016 220)',
  noon: 'oklch(0.985 0.004 240)',
  dusk: 'oklch(0.9 0.045 50)',
  night: 'oklch(0.2 0.03 265)',
}
// Hoàng hôn sang đêm đi qua tông tím chạng vạng thay vì xám đục
export const VIA = { 'dusk>night': 'oklch(0.52 0.075 320)' }

// Ảnh cho các ô danh mục cuối trang
export const CAT_SHOTS = {
  'thiet-bi-bep': { desc: 'Bếp từ, lò nướng, máy rửa bát, tủ lạnh…', shot: { tone: 'morning', src: '/images/cat-bep.jpg' } },
  'gia-dung': { desc: 'Máy giặt, máy sấy, máy pha cà phê…', shot: { tone: 'noon', src: '/images/cat-gia-dung.jpg', pos: '50% 60%' } },
  'khoa-cua': { desc: 'Lắp đặt tận nơi, bảo hành chính hãng', shot: { tone: 'day', still: '/images/lock-el600kb.png' } },
}
