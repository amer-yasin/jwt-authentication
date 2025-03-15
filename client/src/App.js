
import React from "react";
import { Routes, Route } from "react-router-dom";
import Login from "./components/login";
import Private from "./components/private";
import TodoList from "./components/TodoList";
import "bootstrap/dist/css/bootstrap.min.css";

function App() {
  return (
    <div>
      <div className="container-md mt-5">
        <Routes>
          <Route path="/private" element={<Private />} />
          <Route path="/" element={<Login />} />
          <Route path="/todolist" element={<TodoList />} />
        </Routes>
      </div>
    </div>
  );
}

export default App;