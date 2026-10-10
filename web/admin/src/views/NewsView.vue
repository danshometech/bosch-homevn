<script setup>
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Tabs from 'primevue/tabs'
import TabList from 'primevue/tablist'
import Tab from 'primevue/tab'
import TabPanels from 'primevue/tabpanels'
import TabPanel from 'primevue/tabpanel'
import NewsPostsList from '@/components/NewsPostsList.vue'
import NewsCategoriesManager from '@/components/NewsCategoriesManager.vue'

// Bài viết và chuyên mục chung một trang; tab giữ trên URL (?tab=chuyen-muc). lazy: mở tab nào mới tải tab đó
const route = useRoute()
const router = useRouter()
const tab = computed({
  get: () => (route.query.tab === 'chuyen-muc' ? 'categories' : 'posts'),
  set: v => router.replace({ name: 'news', query: v === 'categories' ? { tab: 'chuyen-muc' } : {} }),
})
</script>

<template>
  <div class="page-h">
    <h1>Tin tức</h1>
  </div>
  <Tabs v-model:value="tab" lazy class="page-tabs">
    <TabList>
      <Tab value="posts"><i class="pi pi-book" />Bài viết</Tab>
      <Tab value="categories"><i class="pi pi-tags" />Chuyên mục</Tab>
    </TabList>
    <TabPanels>
      <TabPanel value="posts"><NewsPostsList /></TabPanel>
      <TabPanel value="categories"><NewsCategoriesManager /></TabPanel>
    </TabPanels>
  </Tabs>
</template>
