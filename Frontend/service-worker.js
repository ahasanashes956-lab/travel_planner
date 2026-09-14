const CACHE_NAME = 'travel-planner-bd-v25';
const ASSETS = [
  '/',
  '/index.html',
  '/login.html',
  '/register.html',
  '/forgot-password.html',
  '/reset-password.html',
  '/dashboard.html',
  '/destinations.html',
  '/profile.html',
  '/trips.html',
  '/privacy.html',
  '/manifest.webmanifest',
  '/icon.svg',
  '/images/travel-placeholder.svg',
  '/css/style.css?v=20260909-notification-ui',
  '/css/ui-polish.css?v=20260908-ui',
  '/js/app.js?v=20260911-plan-trip-single',
  '/js/auth.js?v=20260909-notification-flow',
  '/js/dashboard.js?v=20260908-reminders',
  '/js/destinations.js?v=20260911-destination-id-fix',
  '/js/trips.js?v=20260912-expense-payment-fix',
  '/js/profile.js'
  ,'/js/trip-form.js?v=20260911-plan-trip-single'
];

self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(CACHE_NAME).then(cache => cache.addAll(ASSETS)).then(() => self.skipWaiting())
  );
});

self.addEventListener('activate', event => {
  event.waitUntil(
    caches.keys().then(keys => Promise.all(
      keys.filter(key => key !== CACHE_NAME).map(key => caches.delete(key))
    )).then(() => self.clients.claim())
  );
});

self.addEventListener('fetch', event => {
  if (event.request.method !== 'GET') return;

  if (new URL(event.request.url).pathname.startsWith('/api/')) return;

  event.respondWith(
    caches.match(event.request).then(cached => {
      if (cached) return cached;

      return fetch(event.request).then(response => {
        if (!response.ok && response.type !== 'opaque') {
          return caches.match('/images/travel-placeholder.svg');
        }

        const clone = response.clone();
        caches.open(CACHE_NAME).then(cache => cache.put(event.request, clone));
        return response;
      }).catch(() => caches.match('/images/travel-placeholder.svg'));
    })
  );
});
