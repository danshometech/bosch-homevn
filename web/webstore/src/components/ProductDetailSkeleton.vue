<template>
  <div class="wrap" aria-busy="true" aria-label="Đang tải sản phẩm">
    <div class="crumb"><span class="sk sk-line" style="width:260px" /></div>
    <div class="pd">
      <div class="gal">
        <div class="gal-main sk" />
        <div class="gal-thumbs"><span v-for="i in 5" :key="i" class="sk sk-thumb" /></div>
      </div>
      <div>
        <div class="sk-row"><span class="sk sk-tag" /><span class="sk sk-tag" /></div>
        <div class="sk-h1">
          <span class="sk" />
          <span class="sk" style="width:58%" />
        </div>
        <div class="pd-meta">
          <span class="sk sk-line" style="width:140px" />
          <span class="sk sk-line" style="width:110px" />
          <span class="sk sk-line" style="width:90px" />
        </div>
        <div class="pd-chips"><span v-for="w in [76, 64, 88]" :key="w" class="sk sk-pd-chip" :style="{ width: w + 'px' }" /></div>
        <div class="pd-price">
          <span class="sk sk-pd-price" />
          <span class="sk sk-line" style="width:42%;margin-top:12px" />
        </div>
        <div class="sk-row" style="align-items:center;gap:14px">
          <span class="sk sk-line" style="width:64px" />
          <span class="sk sk-btn" style="width:118px" />
          <span class="sk sk-btn" style="width:104px;margin-left:auto" />
        </div>
        <div class="pd-buy"><span class="sk sk-btn" /><span class="sk sk-btn" /></div>
        <div class="perks">
          <div v-for="i in 4" :key="i">
            <span class="sk sk-k" />
            <div class="sk-col"><span class="sk sk-line" style="width:56%" /><span class="sk sk-line" style="width:78%;height:12px" /></div>
          </div>
        </div>
      </div>
    </div>
    <section class="sec">
      <div class="sec-h"><span class="sk sk-h2" /></div>
      <div class="sk-spec">
        <div v-for="i in 6" :key="i"><span class="sk sk-line" style="width:50%" /><span class="sk sk-line" style="width:40%" /></div>
      </div>
    </section>
  </div>
</template>
