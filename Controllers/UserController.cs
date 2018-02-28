using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Autofiller.Models;

namespace Application_WEB_MVC.Controllers
{
    
    [Route("/[controller]")]
    public class UserController : Controller
    {
        private readonly AutofillerContext _context;

        public UserController(AutofillerContext context)
        {
            _context = context;
        }
        
        [HttpPost]
        [Route("/user/{email}")]
        public IActionResult new_user(string email){

            var user = _context.Users
                .Where(u => u.email == email) 
                .FirstOrDefault();

            //Only add website if it does not already exist
            if( user == null){
                User new_user = new User();
                new_user.email = email;
                new_user.created_at = DateTime.Now;
                new_user.updated_at = DateTime.Now;
                _context.Add(new_user);
                _context.SaveChanges();              
                return Ok(new_user);
            }else{
                return StatusCode((int)HttpStatusCode.Conflict);
            }
        }

        //Return all value for user
        [HttpGet]
        [Route("/user/{email}/pivots")]
        public IActionResult get_pivots_user(string email){
            var user = _context.Users
                .Where(u => u.email == email)
                .FirstOrDefault();
            
            if(user == null){
                return StatusCode((int)HttpStatusCode.NotFound);
            }

            var user_values = _context.UserValues
                .Where(u => u.User == user)
                .Include(u => u.Pivot)
                .ToList();
            return Ok(user_values);
        }

        //Post: Create a new value for a user, associated with a pivot
        [HttpPost]
        [Route("{email}/pivot")]        
        public IActionResult New_pivot_user(string email, [FromBody] PivotUserRequest item)
        {
            if (item == null || item.Pivot == null || item.Value == null)
            {
                return BadRequest("You need to give pivot & value for user");
            }

            var user = _context.Users
                .Where(u => u.email == email) 
                .FirstOrDefault();

            if(user == null){
                return NotFound();
            }

            var pivot = _context.Pivots
                .Where(p => p.name == item.Pivot)
                .FirstOrDefault();

            if( pivot != null){
                //If a value already exist for this pivot, forbid it
                var user_value_test = _context.UserValues
                    .Where(uv => uv.Pivot == pivot)
                    .FirstOrDefault();
                if( user_value_test != null){
                    return BadRequest("This pivot already exist for this user");
                }
            }

            var user_value = new UserValue();
            user_value.value = item.Value;
            user_value.created_at = DateTime.Now;
            user_value.updated_at = DateTime.Now;
            user_value.User = user;

            //If this new pivot does not exist at all in DB, add it
            if( pivot == null){
                pivot = new Pivot();
                pivot.name = item.Pivot;
                pivot.created_at = DateTime.Now;
                pivot.updated_at = DateTime.Now;
                _context.Add(pivot);
            }
            //link the new value to existing pivot
            user_value.Pivot = pivot;
            
            _context.Add(user_value);
            _context.SaveChanges();
            return Ok(user_value);
        }

        //Put: set another value for the pivot
        [HttpPut]
        [Route("{email}/pivot")]        
        public IActionResult Maj_pivot(string email, [FromBody] PivotUserRequest item)
        {

            if (item == null || item.Pivot == null || item.Value == null)
            {
                return BadRequest("You need to give pivot & value for user");
            }

            var user = _context.Users
                .Where(u => u.email == email) 
                .FirstOrDefault();

            if(user == null){
                return NotFound();
            }
            
            var pivot = _context.Pivots
                .Where(p => p.name == item.Pivot) 
                .FirstOrDefault();

            if( pivot == null){
                return BadRequest("This pivot does not exist for anybody. Cannot update it.");
            }

            var user_value = _context.UserValues
                .Where(u => u.value == item.Value)
                .Where(u => u.User == user)
                .FirstOrDefault();

            //This value did not exist for user, adding it
            if( user_value == null){
                user_value = new UserValue();
                user_value.value = item.Value;
                user_value.created_at = DateTime.Now;
                user_value.updated_at = DateTime.Now;
                user_value.User = user;
            }
            //Linking user_value to existing pivot
            user_value.Pivot = pivot;
            _context.SaveChanges();
            return Ok(user_value);
        }

        [HttpDelete("{email}")]
        public IActionResult Delete(string email)
        {
            var user = _context.Users
                .Where(u => u.email == email)
                .FirstOrDefault();
            
            if(user == null ){
                return NotFound();
            }

            _context.Users.Remove(user);
            _context.SaveChanges();
            return new NoContentResult();
        }
    }
}