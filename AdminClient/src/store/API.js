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

API.interceptors.response.use(
  (res) => res,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem("profile");
      localStorage.removeItem("userId");
      localStorage.removeItem("logCode");
      window.location.replace("/signIn");
    }
    return Promise.reject(error);
  }
);

export default API;
