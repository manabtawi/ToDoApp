using Microsoft.AspNet.Identity;
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

    [Authorize]
    public class ToDoController : ApiController
    {
        private ToDoEntities db = new ToDoEntities();
        private string userId = string.Empty;

        // GET: api/ToDo
        public IEnumerable<ToDoItem> Get()
        {
            List<ToDoItem> rows = new List<ToDoItem>();
            userId = RequestContext.Principal.Identity.GetUserId();
            foreach (ToDo row in db.ToDoes.Where(x => x.UserId == userId))
            {
                ToDoItem obj = new ToDoItem()
                {
                    Id = row.Id,
                    Name = row.Name,
                    Created = row.Created,
                    Modified = row.Modified,
                    UserId = row.UserId,
                    Notes = row.Notes,
                    Done = row.Done
                };
                rows.Add(obj);
            }
            return rows;
        }

        // GET: api/ToDo/5
        [ResponseType(typeof(ToDoItem))]
        public async Task<IHttpActionResult> Get(int id)
        {
            userId = RequestContext.Principal.Identity.GetUserId();
            ToDo row = await db.ToDoes.FindAsync(id);
            if (row == null)
            {
                return NotFound();
            }
            if (row.UserId != userId)
            {
                return Unauthorized();
            }
            return Ok(new ToDoItem()
            {
                Id = row.Id,
                Name = row.Name,
                Created = row.Created,
                Modified = row.Modified,
                UserId = row.UserId,
                Notes = row.Notes,
                Done = row.Done
            });
        }

        // POST: api/ToDo
        [ResponseType(typeof(ToDoItem))]
        public async Task<IHttpActionResult> Post(ToDoItem model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            ToDo obj = new ToDo()
            {
                Name = model.Name,
                Created = DateTime.Now,
                UserId = RequestContext.Principal.Identity.GetUserId(),
                Notes = model.Notes,
                Done = model.Done
            };
            db.ToDoes.Add(obj);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = obj.Id }, obj);
        }

        // PUT: api/ToDo/5
        [ResponseType(typeof(ToDoItem))]
        public async Task<IHttpActionResult> Put(int id, ToDoItem model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != model.Id)
            {
                return BadRequest();
            }

            userId = RequestContext.Principal.Identity.GetUserId();
            

            ToDo obj = new ToDo()
            {
                Id = id,
                Name = model.Name,
                Modified = DateTime.Now,
                UserId = userId,
                Notes = model.Notes,
                Done = model.Done
            };

            db.Entry(obj).State = EntityState.Modified;

            if (db.ToDoes.Where(x => x.Id == id && x.UserId == userId).SingleOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItemExists(id))
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

        // DELETE: api/ToDo/5
        [ResponseType(typeof(ToDoItem))]
        public async Task<IHttpActionResult> Delete(int id)
        {
            userId = RequestContext.Principal.Identity.GetUserId();
            ToDo row = await db.ToDoes.FindAsync(id);
            if (row == null)
            {
                return NotFound();
            }
            if (row.UserId != userId)
            {
                return Unauthorized();
            }
            db.ToDoes.Remove(row);
            await db.SaveChangesAsync();

            return Ok(row);
        }

        /* protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        } */

        private bool ItemExists(int id)
        {
            return db.ToDoes.Count(e => e.Id == id) > 0;
        }
    }
}
