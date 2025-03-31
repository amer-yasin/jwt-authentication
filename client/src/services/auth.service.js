import axios from "axios";

const baseURL = "http://monday.lcl:5000"; // Base URL for the API
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

  localStorage.removeItem("user");

  return axios.post(
    baseURL + API_URL + "/logout",
    {},
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  );
};

// New method to call the GetCurrentUser API
const fetchCurrentUser = () => {
  const token = JSON.parse(localStorage.getItem("user"))?.accessToken;

  if (!token) {
    return Promise.reject(new Error("No access token found"));
  }

  return axios
    .get(baseURL + API_URL + "/currentUser", {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    })
    .then((response) => response.data)
    .catch((error) => {
      console.error("Error fetching current user:", error);
      throw error;
    });
};

const getCurrentUser = () => {
  return JSON.parse(localStorage.getItem("user"));
};


const authService = {
  login,
  loginWithRefreshToken,
  logout,
  fetchCurrentUser,
  getCurrentUser
};

export default authService;