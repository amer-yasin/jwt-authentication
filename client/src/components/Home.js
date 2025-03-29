import React, { useState, useEffect } from "react";
import PostService from "../services/post.service";
import AuthService from "../services/auth.service";
import { useNavigate, Link } from "react-router-dom";
import Alert from "react-bootstrap/Alert";

const Home = () => {
  const [privatePosts, setPrivatePosts] = useState([]);
  const [username, setUsername] = useState(null); // State to store the username
  const navigate = useNavigate();

  useEffect(() => {
    // Fetch the current user details from the backend
    AuthService.getCurrentUser()
      .then((data) => {
        setUsername(data.username); // Set the username from the API response
      })
      .catch((error) => {
        console.error("Failed to fetch user details:", error);
        AuthService.logout();
        navigate("/");
      });
  }, [navigate]);

  useEffect(() => {
    if (!username) return;

    PostService.getAllPrivatePosts().then(
      (response) => {
        setPrivatePosts(response.data);
      },
      async (error) => {
        // Handle invalid token
        if (error.response == null) {
          const currentUser = AuthService.getCurrentUser();
          if (currentUser) {
            await AuthService.loginWithRefreshToken(currentUser.refreshToken).then(
              () => {
                navigate("/home");
                window.location.reload();
              },
              () => {
                AuthService.logout();
                navigate("/");
                window.location.reload();
              }
            );
          } else {
            AuthService.logout();
            navigate("/");
          }
        }
      }
    );
  }, [username, navigate]);

  const logOut = () => {
    AuthService.logout();
    navigate("/");
  };

  return (
    <div>
      <Alert severity="success">
        <Alert.Heading>
          {username && <h1>Welcome, {username}!</h1>}
        </Alert.Heading>
        {privatePosts}
      </Alert>

      <div className="todo-list-brief">
        <h2>About the Todo List</h2>
        <p>
          The Todo List application allows you to manage your tasks efficiently.
          You can add new tasks, update existing ones, and delete tasks that are no longer needed.
          Each task can be marked as completed or pending. Stay organized and keep track of your daily activities with ease.
        </p>
        <p>
          <Link to="/todolist">Go to Todo List</Link>
        </p>
      </div>

      <div className="d-grid gap-2 mt-3">
        <Link to="/" onClick={logOut}>
          <button type="submit" className="btn btn-primary">
            Logout
          </button>
        </Link>
      </div>
    </div>
  );
};

export default Home;