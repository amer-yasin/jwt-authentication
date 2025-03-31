import React, { useState, useEffect } from "react";
import PostService from "../services/post.service";
import AuthService from "../services/auth.service";
import { useNavigate, Link } from "react-router-dom";
import Alert from 'react-bootstrap/Alert';

const Home = () => {
  const [privatePosts, setPrivatePosts] = useState([]);
  const user = AuthService.getCurrentUser();
  const navigate = useNavigate();

  useEffect(() => {
    if (!user) {
      navigate("/");
      return;
    }

    PostService.getAllPrivatePosts().then(
      (response) => {
        setPrivatePosts(response.data);
      },
      async (error) => {
        // Invalid token
        if (error.response == null) {
          //refresh token
          if (user != null) {
            await AuthService.loginWithRefreshToken(user.refreshToken).then(
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
            window.location.reload();
          }
        }
      }
    );
  }, [user, navigate]);

  const logOut = () => {
    AuthService.logout();
    navigate("/");
  };

  return (
    <div>
      <h1>Home</h1>
      <Alert severity="success">
        <Alert.Heading>      {user && (
        <h1>Welcome, {user.username || user.email}!</h1>
      )}</Alert.Heading>
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