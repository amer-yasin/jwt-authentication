import axios from "axios";

const baseURL = "http://localhost:5000"; // Changed from https to http

const API_URL = "/api/auth";

const login = (email, password) => {
  return axios
    .post(baseURL + API_URL + "/login", {
      email,
      password,
    })
    .then((response) => {
      if (response.data.accessToken) {
        localStorage.setItem("user", JSON.stringify(response.data));
        console.log(response.data);
      }
      return response;
    });
};

const loginWithRefreshToken = (token) => {
  return axios
    .post(baseURL + API_URL + "/refreshToken", {
      token,
    })
    .then((response) => {
      if (response.data.accessToken) {
        localStorage.setItem("user", JSON.stringify(response.data));
        console.log(response.data);
      }
      return response;
    });
};

const logout = () => {
  const user = JSON.parse(localStorage.getItem("user"));
  const token = user?.accessToken;

  return axios.post(baseURL + API_URL + "/logout", {}, {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });
};

const getCurrentUser = () => {
  return JSON.parse(localStorage.getItem("user"));
};

const authService = {
  login,
  loginWithRefreshToken,
  logout,
  getCurrentUser,
};

export default authService;