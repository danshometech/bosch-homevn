function Home(props) {
  const { go } = props;
  const [h, m, s] = useCountdown();
  const P = window.BH_PRODUCTS;
  const flash = P.filter(p => p.tag === 'flash').slice(0, 4);
  const home = P.filter(p => p.cat === 'gia-dung').slice(0, 4);
  const tools = P.filter(p => p.cat !== 'gia-dung').slice(0, 8).filter(p => p.tag !== 'flash').concat(P.filter(p => p.cat === 'do-luong')).slice(0, 4);
  return <div className="wrap" data-screen-label="Trang chủ">
    <section className="hero">
      <div className="hero-main">
        <Ph label="ảnh hero: không gian bếp với thiết bị âm tủ Bosch" />
        <div className="hero-copy">
          <span className="eyebrow">Tuần lễ bếp Đức · đến 31/10</span>
          <h1>Bếp chuẩn Đức, giảm đến 25%</h1>
          <p>Máy rửa bát, bếp từ, lò nướng Bosch chính hãng. Miễn phí lắp đặt và trả góp 0% trong 12 tháng.</p>
          <div style={{ display: 'flex', gap: 10, flexWrap: 'wrap' }}>
            <button className="btn btn-primary" onClick={() => go('list', { cat: 'gia-dung' })}>Mua thiết bị bếp</button>
            <button className="btn btn-ghost" onClick={() => go('list', { cat: null, flash: true })}>Xem flash sale</button>
          </div>
        </div>
      </div>
      <div className="hero-side">
        <div className="hero-tile" onClick={() => go('list', { cat: 'dung-cu' })}>
          <Ph label="ảnh máy khoan pin 18V" />
          <div><h3>Dụng cụ Professional</h3><p>Tặng pin 2.0Ah khi mua bộ 18V</p></div>
        </div>
        <div className="hero-tile" onClick={() => go('list', { cat: 'o-to' })}>
          <Ph label="ảnh ắc quy / gạt mưa" />
          <div><h3>Phụ tùng ô tô</h3><p>Thay ắc quy tận nơi trong 60 phút</p></div>
        </div>
      </div>
    </section>

    <div className="trust">
      {[['✓', 'Chính hãng 100%', 'Tem & hóa đơn VAT'], ['24', 'Bảo hành đến 24 tháng', 'Bảo hành điện tử'], ['0%', 'Trả góp 0% lãi suất', 'Duyệt online 5 phút'], ['30', 'Đổi trả 30 ngày', 'Lỗi do nhà sản xuất']].map(([n, b, s]) =>
        <div key={b}><span className="n" style={{ fontSize: 13, fontWeight: 700 }}>{n}</span><div><b>{b}</b><small>{s}</small></div></div>)}
    </div>

    <section className="sec">
      <div className="sec-h"><div><h2>Danh mục sản phẩm</h2></div></div>
      <div className="cats">
        {window.BH_CATS.map(c => <button key={c.id} className="cat" onClick={() => go('list', { cat: c.id })}>
          <Ph label={c.short.toLowerCase()} />
          <b>{c.name}</b><small>{window.BH_PRODUCTS.filter(p => p.cat === c.id).length} sản phẩm</small>
        </button>)}
      </div>
    </section>

    <section className="flash">
      <div className="flash-h">
        <h2><span className="dot" />Flash sale hôm nay</h2>
        <div className="cd"><span className="cd-l">Kết thúc sau</span><span>{h}</span><i>:</i><span>{m}</span><i>:</i><span>{s}</span></div>
      </div>
      <div className="grid">{flash.map(p => <ProductCard key={p.id} p={p} {...props} flash />)}</div>
    </section>

    <section className="sec">
      <div className="sec-h"><div><h2>Thiết bị gia dụng bán chạy</h2><p>Lắp đặt miễn phí tại Hà Nội, TP. HCM, Đà Nẵng</p></div><button className="link" onClick={() => go('list', { cat: 'gia-dung' })}>Xem tất cả</button></div>
      <div className="grid">{home.map(p => <ProductCard key={p.id} p={p} {...props} />)}</div>
    </section>

    <div className="duo">
      <div className="banner b1"><span className="eyebrow">Trả góp</span><h3>0% lãi suất qua 25 ngân hàng</h3><p>Áp dụng cho đơn từ 3.000.000₫. Không cần trả trước, duyệt online.</p><div><button className="btn btn-dark btn-sm">Tìm hiểu</button></div><span className="big">0%</span></div>
      <div className="banner b2"><span className="eyebrow" style={{ color: 'var(--navy)' }}>Bảo hành</span><h3>Bảo hành điện tử chính hãng</h3><p>Kích hoạt tự động khi giao hàng. Tra cứu bằng số điện thoại.</p><div><button className="btn btn-dark btn-sm">Tra cứu bảo hành</button></div><span className="big">24th</span></div>
    </div>

    <section className="sec">
      <div className="sec-h"><div><h2>Dụng cụ & thiết bị chuyên nghiệp</h2><p>Dòng Professional cho thợ và công trình</p></div><button className="link" onClick={() => go('list', { cat: 'dung-cu' })}>Xem tất cả</button></div>
      <div className="grid">{tools.map(p => <ProductCard key={p.id} p={p} {...props} />)}</div>
    </section>

    <section className="sec" style={{ paddingTop: 0 }}>
      <div className="sec-h"><div><h2>Khách hàng nói gì</h2><p>4.8/5 từ hơn 6.200 đánh giá đã xác minh</p></div></div>
      <div className="revs">{window.BH_REVIEWS.map(r => <div className="rev" key={r.name}>
        <Stars r={r.stars} /><p>"{r.text}"</p>
        <div className="who"><span className="av">{r.name.split(' ').pop()[0]}</span><div><b>{r.name}</b><div className="muted">{r.city} · Đã mua hàng</div></div></div>
      </div>)}</div>
    </section>
  </div>;
}

function Listing(props) {
  const { go, page } = props;
  const { cat, q, flash } = page.params;
  const c = cat && catOf(cat);
  const [subs, setSubs] = useState([]);
  const [price, setPrice] = useState('all');
  const [minR, setMinR] = useState(0);
  const [sort, setSort] = useState('pop');
  const [open, setOpen] = useState(false);
  useEffect(() => { setSubs([]); setPrice('all'); setMinR(0); }, [cat, q, flash]);

  let base = window.BH_PRODUCTS.filter(p => (!cat || p.cat === cat) && (!flash || p.tag === 'flash') && (!q || p.name.toLowerCase().includes(q.toLowerCase()) || p.sub.toLowerCase().includes(q.toLowerCase())));
  const subList = [...new Set(base.map(p => p.sub))];
  const ranges = { all: [0, 1e12], a: [0, 3e6], b: [3e6, 10e6], c: [10e6, 20e6], d: [20e6, 1e12] };
  let items = base.filter(p => (!subs.length || subs.includes(p.sub)) && p.price >= ranges[price][0] && p.price < ranges[price][1] && p.rating >= minR);
  items = [...items].sort((a, b) => sort === 'asc' ? a.price - b.price : sort === 'desc' ? b.price - a.price : sort === 'off' ? pct(b) - pct(a) : b.reviews - a.reviews);
  const title = flash ? 'Flash sale' : q ? `Kết quả cho "${q}"` : c ? c.name : 'Tất cả sản phẩm';

  return <div className="wrap" data-screen-label="Danh sách sản phẩm">
    <div className="crumb"><button onClick={() => go('home')}>Trang chủ</button>/<span style={{ color: 'var(--ink)' }}>{title}</span></div>
    {!c && !flash && !q ? null : null}
    <div className="pills">
      <button className={'pill' + (!cat && !flash && !q ? ' on' : '')} onClick={() => go('list', { cat: null })}>Tất cả</button>
      {window.BH_CATS.map(x => <button key={x.id} className={'pill' + (cat === x.id ? ' on' : '')} onClick={() => go('list', { cat: x.id })}>{x.short}</button>)}
    </div>
    <div className="list">
      <aside className={'filters' + (open ? ' open' : '')}>
        {subList.length > 1 && <div className="f-g"><h6>Loại sản phẩm</h6>
          {subList.map(s => <label className="chk" key={s}><input type="checkbox" checked={subs.includes(s)} onChange={() => setSubs(subs.includes(s) ? subs.filter(x => x !== s) : [...subs, s])} />{s}<small>{base.filter(p => p.sub === s).length}</small></label>)}
        </div>}
        <div className="f-g"><h6>Khoảng giá</h6>
          {[['all', 'Tất cả'], ['a', 'Dưới 3 triệu'], ['b', '3 – 10 triệu'], ['c', '10 – 20 triệu'], ['d', 'Trên 20 triệu']].map(([k, l]) =>
            <label className="chk" key={k}><input type="radio" name="pr" checked={price === k} onChange={() => setPrice(k)} />{l}</label>)}
        </div>
        <div className="f-g"><h6>Đánh giá</h6>
          {[[0, 'Tất cả'], [4.7, 'Từ 4.7 ★'], [4.8, 'Từ 4.8 ★']].map(([k, l]) =>
            <label className="chk" key={k}><input type="radio" name="rt" checked={minR === k} onChange={() => setMinR(k)} />{l}</label>)}
        </div>
        <div className="f-g"><h6>Dịch vụ</h6>
          <label className="chk"><input type="checkbox" defaultChecked />Bảo hành chính hãng</label>
          <label className="chk"><input type="checkbox" />Hỗ trợ trả góp 0%</label>
          <label className="chk"><input type="checkbox" />Lắp đặt miễn phí</label>
        </div>
      </aside>
      <div>
        <div className="toolbar">
          <div><h1>{title}</h1><span className="muted">{items.length} sản phẩm</span></div>
          <div style={{ display: 'flex', gap: 8 }}>
            <button className="btn btn-ghost btn-sm f-btn" onClick={() => setOpen(!open)}>{open ? 'Ẩn bộ lọc' : 'Bộ lọc'}</button>
            <select className="sel" value={sort} onChange={e => setSort(e.target.value)}>
              <option value="pop">Phổ biến nhất</option><option value="off">Giảm nhiều nhất</option><option value="asc">Giá thấp → cao</option><option value="desc">Giá cao → thấp</option>
            </select>
          </div>
        </div>
        {items.length ? <div className="grid">{items.map(p => <ProductCard key={p.id} p={p} {...props} flash={flash} />)}</div>
          : <div className="box empty"><h3>Không tìm thấy sản phẩm phù hợp</h3><p className="muted" style={{ margin: '8px 0 20px' }}>Thử bỏ bớt bộ lọc hoặc tìm với từ khóa khác.</p><button className="btn btn-dark" onClick={() => go('list', { cat: null })}>Xem tất cả sản phẩm</button></div>}
      </div>
    </div>
  </div>;
}

function Detail(props) {
  const { go, page, addCart, cmp, toggleCmp } = props;
  const p = prodOf(page.params.id);
  const c = catOf(p.cat);
  const [img, setImg] = useState(0);
  const [qty, setQty] = useState(1);
  const [tab, setTab] = useState('spec');
  useEffect(() => { setImg(0); setQty(1); window.scrollTo(0, 0); }, [p.id]);
  const related = window.BH_PRODUCTS.filter(x => x.cat === p.cat && x.id !== p.id).slice(0, 4);
  const views = ['mặt trước', 'góc nghiêng', 'chi tiết', 'khi sử dụng'];
  return <div className="wrap" data-screen-label="Chi tiết sản phẩm">
    <div className="crumb"><button onClick={() => go('home')}>Trang chủ</button>/<button onClick={() => go('list', { cat: c.id })}>{c.name}</button>/<span>{p.sub}</span></div>
    <div className="pd">
      <div className="gal">
        <div className="thumbs">{views.map((v, i) => <Ph key={v} label={String(i + 1)} className={img === i ? 'on' : ''} onClick={() => setImg(i)} />)}</div>
        <Ph className="gal-main" label={`ảnh ${p.sub.toLowerCase()} — ${views[img]}`} />
      </div>
      <div>
        <div style={{ display: 'flex', gap: 6 }}><span className="tag">-{pct(p)}%</span>{p.tag === 'new' && <span className="tag new">Mới</span>}<span className="tag ok">Chính hãng</span></div>
        <h1>{p.name}</h1>
        <div className="pd-meta"><Stars r={p.rating} n={p.reviews} /><span>Mã: <span className="mono">{p.id.toUpperCase()}</span></span><span style={{ color: 'var(--ok)', fontWeight: 600 }}>● Còn hàng ({p.stock})</span></div>
        <div className="pd-price">
          <div><b>{fmt(p.price)}</b><s>{fmt(p.old)}</s></div>
          <div className="muted" style={{ fontSize: 13, marginTop: 4 }}>Tiết kiệm {fmt(p.old - p.price)} · Đã gồm VAT</div>
          {p.price >= 3000000 && <div className="inst-box"><span>Trả góp 0% chỉ từ <b>{fmt(Math.round(p.price / 12 / 1000) * 1000)}</b>/tháng</span><button className="link" onClick={() => { addCart(p.id, qty, true); go('checkout', { inst: true }); }}>Mua trả góp</button></div>}
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: 14 }}>
          <span style={{ fontSize: 14, fontWeight: 500 }}>Số lượng</span>
          <div className="qty"><button onClick={() => setQty(Math.max(1, qty - 1))}>−</button><span>{qty}</span><button onClick={() => setQty(Math.min(p.stock, qty + 1))}>+</button></div>
          <button className={'btn btn-ghost btn-sm'} style={{ marginLeft: 'auto' }} onClick={() => toggleCmp(p.id)}><Ic d={I.cmp} size={16} />{cmp.includes(p.id) ? 'Đã thêm so sánh' : 'So sánh'}</button>
        </div>
        <div className="pd-buy">
          <button className="btn btn-ghost" onClick={() => addCart(p.id, qty)}>Thêm vào giỏ</button>
          <button className="btn btn-primary" onClick={() => { addCart(p.id, qty, true); go('cart'); }}>Mua ngay</button>
        </div>
        <div className="perks">
          {[['BH', 'Bảo hành chính hãng 24 tháng', 'Bảo hành điện tử, kích hoạt khi giao hàng'], ['GH', 'Giao hàng & lắp đặt miễn phí', 'Nội thành Hà Nội, TP. HCM, Đà Nẵng trong 24h'], ['ĐT', 'Đổi trả trong 30 ngày', 'Đổi mới nếu lỗi do nhà sản xuất'], ['VAT', 'Xuất hóa đơn VAT', 'Theo yêu cầu cho cá nhân & doanh nghiệp']].map(([k, b, s]) =>
            <div key={k}><span className="k" style={{ fontSize: k.length > 2 ? 9 : 11 }}>{k}</span><div><b>{b}</b><small>{s}</small></div></div>)}
        </div>
      </div>
    </div>

    <section className="sec">
      <div className="tabs">
        <button className={tab === 'spec' ? 'on' : ''} onClick={() => setTab('spec')}>Thông số kỹ thuật</button>
        <button className={tab === 'rev' ? 'on' : ''} onClick={() => setTab('rev')}>Đánh giá ({p.reviews})</button>
      </div>
      {tab === 'spec' ? <table className="spec"><tbody>{Object.entries(p.specs).map(([k, v]) => <tr key={k}><td>{k}</td><td>{v}</td></tr>)}<tr><td>Thương hiệu</td><td>Bosch</td></tr></tbody></table>
        : <div>
          <div className="rv-sum">
            <div><div className="rv-big">{p.rating.toFixed(1)}</div><Stars r={p.rating} /><div className="muted" style={{ fontSize: 13 }}>{p.reviews} đánh giá</div></div>
            <div style={{ maxWidth: 420 }}>{[5, 4, 3, 2, 1].map((s, i) => { const w = [78, 15, 4, 2, 1][i]; return <div className="rv-bar" key={s}><span>{s} ★</span><div className="bar"><i style={{ width: w + '%' }} /></div><span className="muted" style={{ width: 34 }}>{w}%</span></div>; })}</div>
          </div>
          <div className="revs">{window.BH_REVIEWS.map(r => <div className="rev" key={r.name}><Stars r={r.stars} /><p>{r.text}</p><div className="who"><span className="av">{r.name.split(' ').pop()[0]}</span><div><b>{r.name}</b><div className="muted">{r.date} · Đã mua hàng</div></div></div></div>)}</div>
        </div>}
    </section>

    {related.length > 0 && <section style={{ paddingBottom: 24 }}>
      <div className="sec-h"><h2>Sản phẩm tương tự</h2></div>
      <div className="grid">{related.map(x => <ProductCard key={x.id} p={x} {...props} />)}</div>
    </section>}
  </div>;
}

function Cart({ go, cart, setQty, remove }) {
  const lines = Object.entries(cart).map(([id, q]) => ({ p: prodOf(id), q }));
  const sub = lines.reduce((s, l) => s + l.p.price * l.q, 0);
  const save = lines.reduce((s, l) => s + (l.p.old - l.p.price) * l.q, 0);
  const [code, setCode] = useState('');
  const [disc, setDisc] = useState(0);
  const [msg, setMsg] = useState('');
  const apply = () => { if (code.trim().toUpperCase() === 'BOSCH10') { setDisc(Math.min(500000, Math.round(sub * .1))); setMsg('Đã áp dụng mã giảm 10% (tối đa 500.000₫)'); } else { setDisc(0); setMsg('Mã không hợp lệ. Thử BOSCH10'); } };
  if (!lines.length) return <div className="wrap" data-screen-label="Giỏ hàng"><div className="box empty" style={{ marginTop: 32 }}><h2>Giỏ hàng đang trống</h2><p className="muted" style={{ margin: '8px 0 24px' }}>Khám phá các ưu đãi flash sale hôm nay.</p><button className="btn btn-primary" onClick={() => go('home')}>Tiếp tục mua sắm</button></div></div>;
  return <div className="wrap" data-screen-label="Giỏ hàng">
    <div className="crumb"><button onClick={() => go('home')}>Trang chủ</button>/<span>Giỏ hàng</span></div>
    <h1 style={{ fontSize: 28, marginBottom: 20 }}>Giỏ hàng <span className="muted" style={{ fontWeight: 400, fontSize: 18 }}>({lines.reduce((s, l) => s + l.q, 0)})</span></h1>
    <div className="cart">
      <div className="box" style={{ padding: '6px 24px' }}>
        {lines.map(({ p, q }) => <div className="ci" key={p.id}>
          <Ph label={p.sub.toLowerCase()} />
          <div><h4 onClick={() => go('detail', { id: p.id })}>{p.name}</h4><div className="muted" style={{ fontSize: 13 }}>Bảo hành 24 tháng · Còn {p.stock}</div><button className="rm" onClick={() => remove(p.id)}>Xóa</button></div>
          <div style={{ textAlign: 'right', display: 'flex', flexDirection: 'column', alignItems: 'flex-end', gap: 10 }}>
            <div><b style={{ color: 'var(--accent)' }}>{fmt(p.price * q)}</b><div><s className="muted" style={{ fontSize: 12 }}>{fmt(p.old * q)}</s></div></div>
            <div className="qty" style={{ height: 36 }}><button onClick={() => setQty(p.id, q - 1)}>−</button><span>{q}</span><button onClick={() => setQty(p.id, Math.min(p.stock, q + 1))}>+</button></div>
          </div>
        </div>)}
      </div>
      <div className="box" style={{ position: 'sticky', top: 140 }}>
        <h3>Tóm tắt đơn hàng</h3>
        <div className="sum-row"><span>Tạm tính</span><span>{fmt(sub)}</span></div>
        <div className="sum-row"><span>Tiết kiệm</span><span style={{ color: 'var(--ok)' }}>−{fmt(save)}</span></div>
        {disc > 0 && <div className="sum-row"><span>Mã giảm giá</span><span style={{ color: 'var(--ok)' }}>−{fmt(disc)}</span></div>}
        <div className="sum-row"><span>Giao hàng & lắp đặt</span><span>Miễn phí</span></div>
        <div className="coupon"><input placeholder="Mã giảm giá" value={code} onChange={e => setCode(e.target.value)} /><button className="btn btn-ghost btn-sm" style={{ height: 40 }} onClick={apply}>Áp dụng</button></div>
        {msg && <div style={{ fontSize: 12.5, marginTop: -8, marginBottom: 10, color: disc ? 'var(--ok)' : 'var(--accent)' }}>{msg}</div>}
        <div className="sum-row tot"><span>Tổng cộng</span><b>{fmt(sub - disc)}</b></div>
        <div className="muted" style={{ fontSize: 12, textAlign: 'right', marginBottom: 16 }}>Đã bao gồm VAT</div>
        <button className="btn btn-primary btn-block" onClick={() => go('checkout', { disc })}>Tiến hành thanh toán</button>
        {sub >= 3000000 && <button className="btn btn-ghost btn-block" style={{ marginTop: 8 }} onClick={() => go('checkout', { disc, inst: true })}>Trả góp 0% · {fmt(Math.round((sub - disc) / 12 / 1000) * 1000)}/tháng</button>}
      </div>
    </div>
  </div>;
}

function Checkout({ go, page, cart, clearCart }) {
  const lines = Object.entries(cart).map(([id, q]) => ({ p: prodOf(id), q }));
  const disc = page.params.disc || 0;
  const sub = lines.reduce((s, l) => s + l.p.price * l.q, 0);
  const total = sub - disc;
  const [f, setF] = useState({ name: '', phone: '', email: '', city: 'TP. Hồ Chí Minh', addr: '', note: '' });
  const [ship, setShip] = useState('std');
  const [pay, setPay] = useState(page.params.inst && total >= 3000000 ? 'inst' : 'cod');
  const [months, setMonths] = useState(12);
  const [err, setErr] = useState({});
  const [done, setDone] = useState(null);
  const shipFee = ship === 'fast' ? 50000 : 0;
  const upd = k => e => setF({ ...f, [k]: e.target.value });
  const submit = () => {
    const e = {};
    if (!f.name.trim()) e.name = 'Vui lòng nhập họ tên';
    if (!/^0\d{9}$/.test(f.phone.replace(/\s/g, ''))) e.phone = 'Số điện thoại gồm 10 số, bắt đầu bằng 0';
    if (!f.addr.trim()) e.addr = 'Vui lòng nhập địa chỉ';
    setErr(e);
    if (!Object.keys(e).length) { setDone({ id: 'BH' + Math.floor(100000 + Math.random() * 900000), total: total + shipFee, name: f.name, phone: f.phone }); clearCart(); window.scrollTo(0, 0); }
  };
  if (done) return <div className="wrap" data-screen-label="Đặt hàng thành công"><div className="success">
    <div className="ok">✓</div><h1 style={{ fontSize: 30 }}>Đặt hàng thành công</h1>
    <p className="muted" style={{ margin: '10px 0 24px' }}>Cảm ơn {done.name}! Nhân viên sẽ gọi xác nhận qua số {done.phone} trong 15 phút.</p>
    <div className="box" style={{ textAlign: 'left', marginBottom: 24 }}>
      <div className="sum-row"><span>Mã đơn hàng</span><b className="mono">{done.id}</b></div>
      <div className="sum-row"><span>Tổng thanh toán</span><b>{fmt(done.total)}</b></div>
      <div className="sum-row"><span>Bảo hành</span><span>Kích hoạt tự động khi giao hàng</span></div>
    </div>
    <button className="btn btn-primary" onClick={() => go('home')}>Tiếp tục mua sắm</button>
  </div></div>;
  if (!lines.length) return <div className="wrap"><div className="box empty" style={{ marginTop: 32 }}><h2>Chưa có sản phẩm để thanh toán</h2><button className="btn btn-primary" style={{ marginTop: 20 }} onClick={() => go('home')}>Về trang chủ</button></div></div>;
  const Fld = ({ k, label, full, ...rest }) => <label className={'fld' + (full ? ' full' : '') + (err[k] ? ' err' : '')}>{label}<input value={f[k]} onChange={upd(k)} {...rest} />{err[k] && <span className="e">{err[k]}</span>}</label>;
  return <div className="wrap" data-screen-label="Thanh toán">
    <div className="crumb"><button onClick={() => go('home')}>Trang chủ</button>/<button onClick={() => go('cart')}>Giỏ hàng</button>/<span>Thanh toán</span></div>
    <div className="steps"><span className="st"><i>1</i>Giỏ hàng</span><span className="ln" /><span className="st on"><i>2</i>Thông tin & thanh toán</span><span className="ln" /><span className="st"><i>3</i>Hoàn tất</span></div>
    <div className="cart">
      <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
        <div className="box"><h3>Thông tin người nhận</h3>
          <div className="form-g">
            {Fld({ k: 'name', label: 'Họ và tên *', placeholder: 'Nguyễn Văn A' })}
            {Fld({ k: 'phone', label: 'Số điện thoại *', placeholder: '09xx xxx xxx', inputMode: 'tel' })}
            {Fld({ k: 'email', label: 'Email (nhận hóa đơn)', placeholder: 'email@vidu.com' })}
            <label className="fld">Tỉnh / Thành phố<select value={f.city} onChange={upd('city')}>{['TP. Hồ Chí Minh', 'Hà Nội', 'Đà Nẵng', 'Hải Phòng', 'Cần Thơ', 'Tỉnh khác'].map(x => <option key={x}>{x}</option>)}</select></label>
            {Fld({ k: 'addr', label: 'Địa chỉ cụ thể *', placeholder: 'Số nhà, đường, phường/xã', full: true })}
            <label className="fld full">Ghi chú<textarea value={f.note} onChange={upd('note')} placeholder="VD: giao giờ hành chính, cần lắp đặt âm tủ…" /></label>
          </div>
        </div>
        <div className="box"><h3>Giao hàng</h3>
          {[['std', 'Giao tiêu chuẩn + lắp đặt', '2–3 ngày · Kỹ thuật viên lắp đặt tận nơi', 'Miễn phí'], ['fast', 'Giao nhanh trong 24h', 'Áp dụng nội thành HN, HCM, ĐN', fmt(50000)], ['pick', 'Nhận tại showroom', '12 showroom toàn quốc', 'Miễn phí']].map(([k, b, s, r]) =>
            <label key={k} className={'opt' + (ship === k ? ' on' : '')}><input type="radio" checked={ship === k} onChange={() => setShip(k)} /><div><b>{b}</b><small>{s}</small></div><span className="r">{r}</span></label>)}
        </div>
        <div className="box"><h3>Phương thức thanh toán</h3>
          {[['cod', 'Thanh toán khi nhận hàng (COD)', 'Kiểm tra hàng trước khi thanh toán'], ['bank', 'Chuyển khoản / QR ngân hàng', 'VietQR · Xác nhận tự động'], ['wallet', 'Ví điện tử', 'MoMo, ZaloPay, VNPay'], ['card', 'Thẻ quốc tế', 'Visa, Mastercard, JCB'], ...(total >= 3000000 ? [['inst', 'Trả góp 0% qua thẻ tín dụng', '25 ngân hàng · Không cần trả trước']] : [])].map(([k, b, s]) =>
            <React.Fragment key={k}>
              <label className={'opt' + (pay === k ? ' on' : '')}><input type="radio" checked={pay === k} onChange={() => setPay(k)} /><div><b>{b}</b><small>{s}</small></div>{k === 'inst' && <span className="tag" style={{ marginLeft: 'auto', alignSelf: 'center' }}>0%</span>}</label>
              {k === 'inst' && pay === 'inst' && <div className="inst-plans">{[3, 6, 9, 12].map(m => <button key={m} className={months === m ? 'on' : ''} onClick={() => setMonths(m)}><b>{m} tháng</b>{fmt(Math.round((total + shipFee) / m / 1000) * 1000)}/th</button>)}</div>}
            </React.Fragment>)}
        </div>
      </div>
      <div className="box" style={{ position: 'sticky', top: 140 }}>
        <h3>Đơn hàng ({lines.reduce((s, l) => s + l.q, 0)})</h3>
        {lines.map(({ p, q }) => <div className="mini" key={p.id}><div className="ph"><span style={{ fontSize: 9 }}>ảnh</span><span className="q">{q}</span></div><div style={{ flex: 1, lineHeight: 1.35 }}>{p.name}</div><b style={{ whiteSpace: 'nowrap' }}>{fmt(p.price * q)}</b></div>)}
        <div style={{ borderTop: '1px solid var(--line)', marginTop: 10, paddingTop: 10 }}>
          <div className="sum-row"><span>Tạm tính</span><span>{fmt(sub)}</span></div>
          {disc > 0 && <div className="sum-row"><span>Mã giảm giá</span><span style={{ color: 'var(--ok)' }}>−{fmt(disc)}</span></div>}
          <div className="sum-row"><span>Phí giao hàng</span><span>{shipFee ? fmt(shipFee) : 'Miễn phí'}</span></div>
          <div className="sum-row tot"><span>Tổng cộng</span><b>{fmt(total + shipFee)}</b></div>
          {pay === 'inst' && <div className="sum-row"><span>Trả góp {months} tháng</span><b>{fmt(Math.round((total + shipFee) / months / 1000) * 1000)}/th</b></div>}
        </div>
        <button className="btn btn-primary btn-block" style={{ marginTop: 16 }} onClick={submit}>Đặt hàng</button>
        <p className="muted" style={{ fontSize: 12, marginTop: 10, textAlign: 'center' }}>Bằng việc đặt hàng, bạn đồng ý với điều khoản của bosch-homevn.com</p>
      </div>
    </div>
  </div>;
}

Object.assign(window, { Home, Listing, Detail, Cart, Checkout });
