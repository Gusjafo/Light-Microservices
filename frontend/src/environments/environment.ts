const backendBaseUrls = {
  user: 'http://localhost:5001',
  product: 'http://localhost:5003',
  order: 'http://localhost:5005',
  eventHub: 'http://localhost:5007'
} as const;

export const environment = {
  production: false,
  apiUrls: backendBaseUrls,
  signalRHubUrl: `${backendBaseUrls.eventHub}/hub/notifications`
};
