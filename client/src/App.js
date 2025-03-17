
import React from "react";
import { Routes, Route } from "react-router-dom";
import Login from "./components/login";
import Home from "./components/Home";
import TodoList from "./components/TodoList";
import "bootstrap/dist/css/bootstrap.min.css";



function App() {
  return (
    <div>
      <div className="container-md mt-5">
        <Routes>
          <Route path="/" element={<Login />} />
          <Route path="/login" element={<Login />} />
          <Route path="/home" element={<Home />} />
          <Route path="/todoList" element={<TodoList />} />

        </Routes>
      </div>
    </div>
  );
}

export default App;