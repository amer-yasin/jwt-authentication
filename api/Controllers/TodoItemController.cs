using Microsoft.AspNetCore.Mvc;
using api.Models.Entities;
using System.Collections.Generic;
using System.Linq;
using api.Data;
using Microsoft.AspNetCore.Authorization;

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

        // GET: api/TodoItem/user/5
        [HttpGet("user/{userId}")]
        public ActionResult<IEnumerable<TodoItem>> GetTodoItemsByUser(int userId)
        {
            var todoItems = _context.TodoItems.Where(item => item.UserId == userId).ToList();

            if (todoItems == null || !todoItems.Any())
            {
                return NotFound();
            }

            return todoItems;
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