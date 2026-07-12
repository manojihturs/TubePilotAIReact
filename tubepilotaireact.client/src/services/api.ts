import axios from 'axios';

// Create an axios instance with default config
export const apiClient = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// The API wraps every response in an envelope: { success, message, data, meta }.
// Unwrap it here so service methods can keep treating response.data as the payload.
apiClient.interceptors.response.use(
  (response) => {
    if (response.data && typeof response.data === 'object' && 'success' in response.data && 'data' in response.data) {
      response.data = response.data.data;
    }
    return response;
  },
  (error) => {
    console.error('API Error:', error);
    return Promise.reject(error);
  }
);

export default apiClient;
