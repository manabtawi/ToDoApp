using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ToDoApp.EF;
using ToDoApp.Models;

namespace ToDoApp.Controllers
{
    public class ToDoTestController : ApiController
    {
        private ToDoEntities db = new ToDoEntities();

        // GET: api/ToDoTest
        public IQueryable<ToDoItem> GetToDoItems()
        {
            return db.ToDoItems;
        }

        // GET: api/ToDoTest/5
        [ResponseType(typeof(ToDoItem))]
        public async Task<IHttpActionResult> GetToDoItem(int id)
        {
            ToDoItem toDoItem = await db.ToDoItems.FindAsync(id);
            if (toDoItem == null)
            {
                return NotFound();
            }

            return Ok(toDoItem);
        }

        // PUT: api/ToDoTest/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutToDoItem(int id, ToDoItem toDoItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != toDoItem.Id)
            {
                return BadRequest();
            }

            //db.Entry(toDoItem).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ToDoItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/ToDoTest
        [ResponseType(typeof(ToDoItem))]
        public async Task<IHttpActionResult> PostToDoItem(ToDoItem toDoItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ToDoItems.Add(toDoItem);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = toDoItem.Id }, toDoItem);
        }

        // DELETE: api/ToDoTest/5
        [ResponseType(typeof(ToDoItem))]
        public async Task<IHttpActionResult> DeleteToDoItem(int id)
        {
            ToDoItem toDoItem = await db.ToDoItems.FindAsync(id);
            if (toDoItem == null)
            {
                return NotFound();
            }

            db.ToDoItems.Remove(toDoItem);
            await db.SaveChangesAsync();

            return Ok(toDoItem);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ToDoItemExists(int id)
        {
            return db.ToDoItems.Count(e => e.Id == id) > 0;
        }
    }
}