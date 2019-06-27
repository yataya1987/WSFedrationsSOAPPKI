﻿using DevExpress.EntityFramework.SecurityDataStore.Authorization;
using EFCoreSecurityODataService.DataModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.OData;
using System.Web.OData.Query;
using System.Collections.Generic;
using System.Web.OData.Extensions;
using DevExpress.EntityFramework.SecurityDataStore.Security;

namespace EFCoreSecurityODataService.Controllers {
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ContactsController : ODataController {
        private EFCoreDemoDbContext contactContext = new EFCoreDemoDbContext(PermissionsProviderContext.GetPermissionsProvider());
        private bool ContactExists(int key) {
            return contactContext.Contacts.Any(p => p.Id == key);
        }
        protected override void Dispose(bool disposing) {
            contactContext.Dispose();
            base.Dispose(disposing);
        }
        [EnableQuery]
        public IQueryable<Contact> Get() {
            IQueryable<Contact> result = contactContext.Contacts
                    .Include(c => c.Department)
                    .Include(c => c.ContactTasks)
                    .ThenInclude(ct => ct.Task);
            return result;
        }
        [EnableQuery]
        public IQueryable<Contact> Get([FromODataUri] int key) {
            IQueryable<Contact> result = contactContext.Contacts.
                Where(p => p.Id == key).
                Include(p => p.Department).
                Include(c => c.ContactTasks).
                ThenInclude(ct => ct.Task).
                ToArray().
                AsQueryable();
            return result;
        }
        public async Task<IHttpActionResult> Post(Contact contact) {
            if(!ModelState.IsValid) {
                return BadRequest(ModelState);
            }
            contactContext.Contacts.Add(contact);
            await contactContext.SaveChangesAsync();
            return Created(contact);
        }
        public async Task<IHttpActionResult> Patch([FromODataUri] int key, Delta<Contact> contact) {
            if(!ModelState.IsValid) {
                return BadRequest(ModelState);
            }
            var entity = await contactContext.Contacts.FirstOrDefaultAsync(p => p.Id == key);
            if(entity == null) {
                return NotFound();
            }
            contact.Patch(entity);
            try {
                await contactContext.SaveChangesAsync();
            }
            catch(DbUpdateConcurrencyException) {
                if(!ContactExists(key)) {
                    return NotFound();
                }
                else {
                    throw;
                }
            }
            return Updated(contact);
        }
        public async Task<IHttpActionResult> Put([FromODataUri] int key, Contact contact) {
            if(!ModelState.IsValid) {
                return BadRequest(ModelState);
            }
            if(key != contact.Id) {
                return BadRequest();
            }
            contactContext.Entry(contact).State = EntityState.Modified;
            try {
                await contactContext.SaveChangesAsync();
            }
            catch(DbUpdateConcurrencyException) {
                if(!ContactExists(key)) {
                    return NotFound();
                }
                else {
                    throw;
                }
            }
            return Updated(contact);
        }
        public async Task<IHttpActionResult> Delete([FromODataUri] int key) {
            var contact = await contactContext.Contacts.FirstOrDefaultAsync(p => p.Id == key);
            if(contact == null) {
                return NotFound();
            }
            contactContext.Contacts.Remove(contact);
            await contactContext.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
        [EnableQuery]
        public IQueryable<Department> GetDepartment([FromODataUri] int key) {
            IQueryable<Department> result = Enumerable.Empty<Department>().AsQueryable();
            IQueryable<Contact> contacts = contactContext.Contacts
                .Include(c => c.Department).Include(c => c.ContactTasks).ThenInclude(ct => ct.Task).Where(c => c.Id == key);
            if(contacts.Count() > 0) {
                Contact contact = contacts.First();
                if(contact.Department != null) {
                    result = contactContext.Departments
                            .Include(p => p.Contacts)
                            .ThenInclude(c => c.ContactTasks)
                            .ThenInclude(ct => ct.Task)
                            .Where(d => d.Id == contact.Department.Id);
                }
            }
            return result;
        }
        [EnableQuery]
        public IQueryable<ContactTask> GetContactTasks([FromODataUri] int key) {
            IQueryable<ContactTask> result = contactContext.ContactTasks
                .Include(ct => ct.Task)
                .Include(ct => ct.Contact)
                .ThenInclude(c => c.Department)
                .Where(ct => ct.Contact.Id == key);
            return result;
        }
        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key, string navigationProperty, [FromBody] Uri link) {
            Contact contact = await contactContext.Contacts.SingleOrDefaultAsync(p => p.Id == key);
            if(contact == null) {
                return NotFound();
            }
            switch(navigationProperty) {
                case "Department":
                    int relatedKey = Helpers.GetKeyFromUri<int>(Request, link);
                    Department department = await contactContext.Departments.SingleOrDefaultAsync(p => p.Id == relatedKey);
                    if(department == null) {
                        return NotFound();
                    }
                    contact.Department = department;
                    break;
                case "ContactTasks":
                    relatedKey = Helpers.GetKeyFromUri<int>(Request, link);
                    ContactTask task = await contactContext.ContactTasks.SingleOrDefaultAsync(p => p.Id == relatedKey);
                    if(task == null) {
                        return NotFound();
                    }
                    contact.ContactTasks.Add(task);
                    break;
                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await contactContext.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
        public async Task<IHttpActionResult> DeleteRef([FromODataUri] int key, string navigationProperty, [FromBody] Uri link) {
            Contact contact = await contactContext.Contacts.SingleOrDefaultAsync(p => p.Id == key);
            if(contact == null) {
                return NotFound();
            }
            switch(navigationProperty) {
                case "Department":
                    contact.Department = null;
                    break;
                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await contactContext.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}