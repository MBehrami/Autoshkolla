import axios from "axios";

const API = axios.create({ baseURL: process.env.APIURL });

API.interceptors.request.use((req) => {
  if (localStorage.getItem("profile")) {
    req.headers.Authorization = `Bearer ${
      JSON.parse(localStorage.getItem("profile")).token
    }`;
  }
  return req;
});

// 401 handling is done in App.vue's global interceptor to avoid
// full-page reloads that cause infinite refresh loops.

export default API;
