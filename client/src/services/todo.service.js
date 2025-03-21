import axios from 'axios';
import authHeader from './auth-header';

const baseURL = "http://monday.lcl:5000"; // Changed from https to http

const API_URL = '/api/TodoItem/';

class TodoService {
    getTodoItems() {
        return axios.get(baseURL + API_URL, { headers: authHeader() });
    }

    getTodoItem(id) {
        return axios.get(baseURL + API_URL + id, { headers: authHeader() });
    }

    getTodoItemsByUser(userId) {
        return axios.get(baseURL + API_URL + 'user/' + userId, { headers: authHeader() });
    }

    createTodoItem(todoItem) {
        return axios.post(baseURL + API_URL, todoItem, { headers: authHeader() });
    }

    updateTodoItem(id, todoItem) {
        return axios.put(baseURL + API_URL + id, todoItem, { headers: authHeader() });
    }

    deleteTodoItem(id) {
        return axios.delete(baseURL + API_URL + id, { headers: authHeader() });
    }
}

export default new TodoService();