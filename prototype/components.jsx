const { useState, useEffect, useMemo, useRef } = React;

const fmt = n => n.toLocaleString('vi-VN') + '₫';
const pct = p => Math.round((1 - p.price / p.old) * 100);
const catOf = id => window.BH_CATS.find(c => c.id === id);
const prodOf = id => window.BH_PRODUCTS.find(p => p.id === id);

function Ic({ d, size = 20 }) {
  return <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round">{d}</svg>;
}
const I = {
  cart: <><path d="M3 4h2l2.2 11h11L21 7H6.2" /><circle cx="9" cy="20" r="1.3" /><circle cx="17" cy="20" r="1.3" /></>,
  user: <><circle cx="12" cy="8" r="4" /><path d="M4 21c1.5-4 4.5-6 8-6s6.5 2 8 6" /></>,
  phone: <path d="M5 4h4l2 5-2.5 1.5a11 11 0 0 0 5 5L15 13l5 2v4a2 2 0 0 1-2 2A16 16 0 0 1 3 6a2 2 0 0 1 2-2" />,
  pin: <><path d="M12 21s-7-6.5-7-12a7 7 0 0 1 14 0c0 5.5-7 12-7 12z" /><circle cx="12" cy="9" r="2.5" /></>,
  cmp: <><path d="M7 4v16M17 4v16M3 8l4-4 4 4M13 16l4 4 4-4" /></>,
};

function Ph({ label, className = '', style, onClick }) {
  return <div className={'ph ' + className} style={style} onClick={onClick}><span>{label}</span></div>;
}

function Stars({ r, n }) {
  const full = Math.round(r);
  return <div className="stars"><span className="s">{'★'.repeat(full)}<span style={{ color: '#ddd' }}>{'★'.repeat(5 - full)}</span></span>{r.toFixed(1)}{n != null && <span>({n})</span>}</div>;
}

function useCountdown() {
  const end = useMemo(() => { const d = new Date(); d.setHours(23, 59, 59, 0); return d.getTime(); }, []);
  const [now, setNow] = useState(Date.now());
  useEffect(() => { const t = setInterval(() => setNow(Date.now()), 1000); return () => clearInterval(t); }, []);
  const s = Math.max(0, Math.floor((end - now) / 1000));
  return [Math.floor(s / 3600), Math.floor(s / 60) % 60, s % 60].map(x => String(x).padStart(2, '0'));
}

function Header({ go, page, cartCount, query, setQuery }) {
  const [q, setQ] = useState(query || '');
  useEffect(() => setQ(query || ''), [query]);
  const submit = e => { e.preventDefault(); setQuery(q); go('list', { cat: null, q }); };
  return <>
    <div className="topbar"><div className="wrap">
      <span>Đại lý phân phối <b>chính hãng Bosch</b> · Bảo hành toàn quốc</span>
      <span className="hide-m">Hotline <b>1900 6868</b> · Miễn phí giao lắp nội thành</span>
    </div></div>
    <header className="header">
      <div className="wrap header-main">
        <div className="logo" onClick={() => go('home')}><b>bosch<i>-</i>home<i>vn</i></b><small>Đại lý ủy quyền</small></div>
        <form className="search" onSubmit={submit}>
          <input value={q} onChange={e => setQ(e.target.value)} placeholder="Tìm máy rửa bát, máy khoan, ắc quy…" />
          <button type="submit">Tìm</button>
        </form>
        <div className="h-actions">
          <button className="h-act hide-m"><span className="ic"><Ic d={I.pin} /></span><span className="lbl"><small>Hệ thống</small>12 showroom</span></button>
          <button className="h-act hide-m"><span className="ic"><Ic d={I.user} /></span><span className="lbl"><small>Xin chào</small>Đăng nhập</span></button>
          <button className="h-act" onClick={() => go('cart')}><span className="ic"><Ic d={I.cart} /></span>{cartCount > 0 && <span className="badge">{cartCount}</span>}<span className="lbl"><small>Giỏ hàng</small>{cartCount} sản phẩm</span></button>
        </div>
      </div>
      <div className="wrap"><nav className="nav">
        <button className="hot" onClick={() => go('list', { cat: null, flash: true })}>Flash sale</button>
        {window.BH_CATS.map(c => <button key={c.id} className={page.name === 'list' && page.params.cat === c.id ? 'on' : ''} onClick={() => go('list', { cat: c.id })}>{c.name}</button>)}
        <button>Trả góp 0%</button>
        <button>Bảo hành</button>
      </nav></div>
    </header>
  </>;
}

function ProductCard({ p, go, addCart, cmp, toggleCmp, flash }) {
  const on = cmp.includes(p.id);
  const sold = Math.min(92, 100 - p.stock * 2);
  return <div className="card">
    <div className="card-tags">
      <span className="tag">-{pct(p)}%</span>
      {p.tag === 'new' && <span className="tag new">Mới</span>}
    </div>
    <button className={'card-cmp' + (on ? ' on' : '')} onClick={() => toggleCmp(p.id)}>{on ? '✓ Đã chọn' : '+ So sánh'}</button>
    <Ph label={'ảnh ' + p.sub.toLowerCase()} onClick={() => go('detail', { id: p.id })} />
    <div className="card-sub">{p.sub}</div>
    <h4 onClick={() => go('detail', { id: p.id })}>{p.name}</h4>
    <Stars r={p.rating} n={p.reviews} />
    <div className="price"><b>{fmt(p.price)}</b><s>{fmt(p.old)}</s></div>
    {flash
      ? <div className="stock"><div className="bar"><i style={{ width: sold + '%' }} /></div><small>Đã bán {sold}% · còn {p.stock} sản phẩm</small></div>
      : p.price >= 3000000 && <div className="inst">Trả góp 0%: <b>{fmt(Math.round(p.price / 12 / 1000) * 1000)}</b>/tháng</div>}
    <button className="btn btn-dark btn-sm" onClick={() => addCart(p.id)}>Thêm vào giỏ</button>
  </div>;
}

function Footer({ go }) {
  return <footer className="footer"><div className="wrap">
    <div className="footer-g">
      <div>
        <div className="logo" onClick={() => go('home')}><b>bosch<i>-</i>home<i>vn</i></b><small>Đại lý ủy quyền</small></div>
        <p className="muted" style={{ marginTop: 14, maxWidth: 300 }}>Phân phối thiết bị gia dụng, dụng cụ điện và phụ tùng Bosch chính hãng. Hóa đơn VAT, bảo hành điện tử toàn quốc.</p>
        <div className="pay">{['VISA', 'MASTERCARD', 'JCB', 'MOMO', 'VNPAY', 'ZALOPAY', 'COD'].map(x => <span key={x}>{x}</span>)}</div>
      </div>
      <div><h5>Sản phẩm</h5><ul>{window.BH_CATS.map(c => <li key={c.id} onClick={() => go('list', { cat: c.id })}>{c.name}</li>)}</ul></div>
      <div><h5>Chính sách</h5><ul><li>Bảo hành chính hãng</li><li>Đổi trả 30 ngày</li><li>Trả góp 0%</li><li>Giao hàng & lắp đặt</li><li>Bảo mật thông tin</li></ul></div>
      <div><h5>Liên hệ</h5><ul><li>Hotline: 1900 6868</li><li>Zalo OA: bosch-homevn</li><li>cskh@bosch-homevn.com</li><li>8:00 – 21:00, cả CN</li></ul></div>
    </div>
    <div className="footer-b"><span>© 2026 bosch-homevn.com · Đại lý phân phối được ủy quyền.</span><span>MST 0123456789 · ĐKKD do Sở KH&ĐT cấp</span></div>
  </div></footer>;
}

function FloatContact() {
  return <div className="float">
    <button className="fab zalo"><span className="c">Zalo</span><span className="t">Chat tư vấn</span></button>
    <button className="fab hot"><span className="c"><Ic d={I.phone} size={18} /></span><span className="t">1900 6868</span></button>
  </div>;
}

function CompareTray({ cmp, toggleCmp, open, clear }) {
  if (!cmp.length) return null;
  return <div className="cmp-tray">
    <b style={{ fontSize: 13, whiteSpace: 'nowrap' }}>So sánh ({cmp.length}/4)</b>
    <div className="chips">{cmp.map(id => <span className="chip" key={id}><span style={{ overflow: 'hidden', textOverflow: 'ellipsis' }}>{prodOf(id).name}</span><button onClick={() => toggleCmp(id)}>✕</button></span>)}</div>
    <button className="btn btn-sm" style={{ color: '#aaa' }} onClick={clear}>Xóa</button>
    <button className="btn btn-primary btn-sm" disabled={cmp.length < 2} style={{ opacity: cmp.length < 2 ? .5 : 1 }} onClick={open}>So sánh ngay</button>
  </div>;
}

function CompareModal({ cmp, close, addCart, go }) {
  const ps = cmp.map(prodOf);
  const keys = [...new Set(ps.flatMap(p => Object.keys(p.specs)))];
  const best = Math.min(...ps.map(p => p.price));
  return <div className="modal-bg" onClick={close}><div className="modal" onClick={e => e.stopPropagation()}>
    <div className="modal-h"><h2 style={{ fontSize: 22 }}>So sánh sản phẩm</h2><button className="btn btn-ghost btn-sm" onClick={close}>Đóng ✕</button></div>
    <div style={{ overflowX: 'auto' }}><table className="cmp-table">
      <tbody>
        <tr><th></th>{ps.map(p => <td key={p.id}><Ph label={p.sub.toLowerCase()} /><b style={{ fontSize: 14, fontWeight: 600, cursor: 'pointer' }} onClick={() => { close(); go('detail', { id: p.id }); }}>{p.name}</b></td>)}</tr>
        <tr><th>Giá</th>{ps.map(p => <td key={p.id}><b style={{ color: 'var(--accent)' }}>{fmt(p.price)}</b>{p.price === best && ps.length > 1 && <div><span className="tag ok" style={{ display: 'inline-block', marginTop: 6 }}>Giá tốt nhất</span></div>}</td>)}</tr>
        <tr><th>Đánh giá</th>{ps.map(p => <td key={p.id}><Stars r={p.rating} n={p.reviews} /></td>)}</tr>
        {keys.map(k => <tr key={k}><th>{k}</th>{ps.map(p => <td key={p.id}>{p.specs[k] || '—'}</td>)}</tr>)}
        <tr><th></th>{ps.map(p => <td key={p.id}><button className="btn btn-dark btn-sm" onClick={() => addCart(p.id)}>Thêm vào giỏ</button></td>)}</tr>
      </tbody>
    </table></div>
  </div></div>;
}

Object.assign(window, { fmt, pct, catOf, prodOf, Ic, I, Ph, Stars, useCountdown, Header, ProductCard, Footer, FloatContact, CompareTray, CompareModal });
