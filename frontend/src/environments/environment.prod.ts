const backendBaseUrls = {
  user: 'https://user-api.example.com',
  product: 'https://product-api.example.com',
  order: 'https://order-api.example.com',
  eventHub: 'https://eventhub.example.com'
} as const;

export const environment = {
  production: true,
  apiUrls: backendBaseUrls,
  signalRHubUrl: `${backendBaseUrls.eventHub}/hub/notifications`
};
