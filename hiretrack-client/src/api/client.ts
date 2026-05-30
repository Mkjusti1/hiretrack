import axios from 'axios'

const isProduction = import.meta.env.PROD
const baseURL = isProduction
  ? 'https://hiretrack-api-46e7.onrender.com'
  : ''

const client = axios.create({ baseURL })

client.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

client.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err.response?.status === 401) {
      localStorage.removeItem('token')
      window.location.href = '/login'
    }
    return Promise.reject(err)
  }
)

export default client