'use client';

import { useState, useEffect } from 'react';
import axios from 'axios';

const API_BASE_URL = 'http://localhost:5001/api/todos';

export default function Home() {
  const [todos, setTodos] = useState([]);
  const [newTodo, setNewTodo] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const fetchTodos = async () => {
    try {
      setLoading(true);
      const response = await axios.get(API_BASE_URL);
      setTodos(response.data);
      setError(null);
    } catch (error) {
      console.error('Error fetching todos:', error);
      setError('Failed to load todos');
    } finally {
      setLoading(false);
    }
  };

  const addTodo = async (e) => {
    e.preventDefault();
    if (!newTodo.trim()) return;
    
    try {
      await axios.post(API_BASE_URL, { title: newTodo });
      setNewTodo('');
      fetchTodos();
    } catch (error) {
      console.error('Error adding todo:', error);
      setError('Failed to add todo');
    }
  };

  const toggleTodo = async (id, completed) => {
    try {
      await axios.patch(`${API_BASE_URL}/${id}`, { completed: !completed });
      fetchTodos();
    } catch (error) {
      console.error('Error toggling todo:', error);
      setError('Failed to update todo');
    }
  };

  const deleteTodo = async (id) => {
    try {
      await axios.delete(`${API_BASE_URL}/${id}`);
      fetchTodos();
    } catch (error) {
      console.error('Error deleting todo:', error);
      setError('Failed to delete todo');
    }
  };

  useEffect(() => {
    fetchTodos();
  }, []);

  return (
    <div className="min-h-screen bg-gray-100 py-8 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md mx-auto bg-white rounded-lg shadow-lg overflow-hidden">
        <div className="px-4 py-5 sm:p-6">
          <h1 className="text-3xl font-bold text-gray-900 text-center mb-8">
            Todo App
          </h1>

          {error && (
            <div className="mb-4 p-4 text-red-700 bg-red-100 rounded-lg">
              {error}
            </div>
          )}
          
          <form onSubmit={addTodo} className="mb-6">
            <div className="flex gap-2">
              <input
                type="text"
                value={newTodo}
                onChange={(e) => setNewTodo(e.target.value)}
                placeholder="What needs to be done?"
                className="todo-input"
                disabled={loading}
              />
              <button 
                type="submit"
                className="btn-primary"
                disabled={loading || !newTodo.trim()}
              >
                Add
              </button>
            </div>
          </form>

          {loading ? (
            <div className="text-center text-gray-500">Loading...</div>
          ) : (
            <ul className="divide-y divide-gray-200">
              {todos.map((todo) => (
                <li 
                  key={todo._id} 
                  className="py-4 flex items-center justify-between gap-4 group hover:bg-gray-50 px-2 rounded-lg transition-colors duration-200"
                >
                  <div className="flex items-center gap-3 flex-1">
                    <input
                      type="checkbox"
                      checked={todo.completed}
                      onChange={() => toggleTodo(todo._id, todo.completed)}
                      className="h-5 w-5 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                    />
                    <span className={`flex-1 text-gray-900 ${todo.completed ? 'line-through text-gray-500' : ''}`}>
                      {todo.title}
                    </span>
                  </div>
                  <button
                    onClick={() => deleteTodo(todo._id)}
                    className="opacity-0 group-hover:opacity-100 btn-danger text-sm"
                  >
                    Delete
                  </button>
                </li>
              ))}
            </ul>
          )}

          {!loading && todos.length === 0 && (
            <div className="text-center text-gray-500 py-4">
              No todos yet. Add one above!
            </div>
          )}
        </div>
      </div>
    </div>
  );
} 