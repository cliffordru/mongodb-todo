# Todo Application

A full-stack todo application with interchangeable Node.js and .NET backends, React/Next.js frontend, and MongoDB database.

## Project Structure 

## Switching Between Backends

The frontend is configured to work with either backend. To switch between them:

1. Stop the currently running backend
2. Start the desired backend (Node.js or .NET)
3. Update the API base URL in the frontend environment:
   - For Node.js backend: `NEXT_PUBLIC_API_URL=http://localhost:3001`
   - For .NET backend: `NEXT_PUBLIC_API_URL=http://localhost:5000`

## API Endpoints

Both backends implement the same REST API endpoints:

- `GET /api/todos` - Get all todos
- `POST /api/todos` - Create a new todo
- `PUT /api/todos/:id` - Update a todo
- `DELETE /api/todos/:id` - Delete a todo

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details. 