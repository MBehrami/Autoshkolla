import axios from "axios";

const API = axios.create({ baseURL: import.meta.env.VITE_API_URL });

API.interceptors.request.use((req) => {
  try {
    const raw = localStorage.getItem("profile");
    if (raw) {
      const profile = JSON.parse(raw);
      if (profile?.token) {
        req.headers.Authorization = `Bearer ${profile.token}`;
      }
    }
  } catch {
    localStorage.removeItem("profile");
  }
  return req;
});

export default API;
