import React, { useState, useEffect } from 'react';
import TodoService from '../services/todo.service';
import './TodoList.css';

const TodoList = () => {
    const [todoItems, setTodoItems] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [newItemTitle, setNewItemTitle] = useState('');

    useEffect(() => {
        const fetchTodoItems = async () => {
            try {
                const response = await TodoService.getTodoItems(); // Replace '1' with the current user's ID
                setTodoItems(response.data);
            } catch (err) {
                setError(err);
            } finally {
                setLoading(false);
            }
        };

        fetchTodoItems();
    }, []);

    const handleUpdate = async (id, updatedItem) => {
        try {
            await TodoService.updateTodoItem(id, updatedItem);
            setTodoItems(todoItems.map(item => (item.id === id ? updatedItem : item)));
        } catch (err) {
            setError(err);
        }
    };

    const handleAdd = async () => {
        try {
                        const newItem = { title: newItemTitle, completed: false, userid: 1194 };
            const response = await TodoService.createTodoItem(newItem);
            setTodoItems([...todoItems, response.data]);
            setNewItemTitle('');
        } catch (err) {
            setError(err);
        }
    };

    const handleDelete = async (id) => {
        try {
            await TodoService.deleteTodoItem(id);
            setTodoItems(todoItems.filter(item => item.id !== id));
        } catch (err) {
            setError(err);
        }
    };

    if (loading) return <p>Loading...</p>;
    if (error) return <p>Error: {error.message}</p>;

    return (
        <div className="todo-list-container">
            <h1>Todo List</h1>
            <input
                type="text"
                value={newItemTitle}
                onChange={(e) => setNewItemTitle(e.target.value)}
                placeholder="New todo item"
            />
            <button onClick={handleAdd}>Add</button>
            <ul>
                {todoItems.map(item => (
                    <li key={item.id}>
                        <input
                            type="text"
                            value={item.title}
                            onChange={(e) => handleUpdate(item.id, { ...item, title: e.target.value })}
                        />
                        <input
                            type="checkbox"
                            checked={item.completed}
                            onChange={(e) => handleUpdate(item.id, { ...item, completed: e.target.checked })}
                        />
                        <button onClick={() => handleDelete(item.id)}>Delete</button>
                    </li>
                ))}
            </ul>
        </div>
    );
};

export default TodoList;