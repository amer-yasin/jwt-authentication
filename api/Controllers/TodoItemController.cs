using Microsoft.AspNetCore.Mvc;
using api.Models.Entities;
using System.Collections.Generic;
using System.Linq;
using api.Data;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Security.Claims;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Ensure all actions require authentication
    public class TodoItemController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TodoItemController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/TodoItem
        [HttpGet]
        public ActionResult<IEnumerable<TodoItem>> GetTodoItems()
        {
            return _context.TodoItems.ToList();
        }

        // GET: api/TodoItem/5
        [HttpGet("{id}")]
        public ActionResult<TodoItem> GetTodoItem(int id)
        {
            var todoItem = _context.TodoItems.Find(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            return todoItem;
        }

        // GET: api/TodoItem/user/{userId}
        [HttpGet("user/{userId}")]
        public ActionResult<IEnumerable<TodoItem>> GetTodoItemsByUser(int userId)
        {
            // Access the NameIdentifier claim directly
            var authenticatedUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (authenticatedUserId == null || !int.TryParse(authenticatedUserId, out var userIdFromToken))
            {
                return Unauthorized(); // Return 401 Unauthorized if the claim is missing or invalid
            }

            // Validate that the userId matches the authenticated user's ID
            if (userId != userIdFromToken)
            {
                return Forbid(); // Return 403 Forbidden if the userId does not match
            }

            // Fetch the Todo items for the authenticated user
            var todoItems = _context.TodoItems.Where(item => item.UserId == userId).ToList();

            if (todoItems == null || !todoItems.Any())
            {
                return Ok(new List<TodoItem>()); // Return an empty list instead of NotFound
            }

            return Ok(todoItems);
        }

        // POST: api/TodoItem
        [HttpPost]
        public ActionResult<TodoItem> PostTodoItem(TodoItem todoItem)
        {
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
        }

        // PUT: api/TodoItem/5
        [HttpPut("{id}")]
        public IActionResult PutTodoItem(int id, TodoItem todoItem)
        {
            if (id != todoItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(todoItem).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _context.SaveChanges();

            return NoContent();
        }

        // DELETE: api/TodoItem/5
        [HttpDelete("{id}")]
        public IActionResult DeleteTodoItem(int id)
        {
            var todoItem = _context.TodoItems.Find(id);
            if (todoItem == null)
            {
                return NotFound();
            }

            _context.TodoItems.Remove(todoItem);
            _context.SaveChanges();

            return NoContent();
        }
    }
}