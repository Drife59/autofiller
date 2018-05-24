using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

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
        private readonly IConfiguration _iconfiguration;
        private readonly ILogger _logger;

        public UserController(AutofillerContext context,
                              ILogger<UserController> logger,
                              IConfiguration iconfiguration)
        {
            _context = context;
            _iconfiguration = iconfiguration;
            _logger = logger;
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

        //Return a value associated with a pivot for user
        [HttpGet]
        [Route("/user/{email}/pivot/{pivot_name}")]
        public IActionResult get_value_user(string email, string pivot_name){
            var user = _context.Users
                .Where(u => u.email == email)
                .FirstOrDefault();
            
            if(user == null){
                return StatusCode((int)HttpStatusCode.NotFound);
            }

            var user_value = _context.UserValues
                .Where(u => u.User == user)
                .Include(u => u.Pivot)
                //.Where(u => u.Pivot.restitution_enabled == true)
                .Where(u => u.Pivot.name == pivot_name)
                .FirstOrDefault();

            if( user_value == null){
                Console.Write("Pas de user value pour ce pivot");
                return StatusCode((int)HttpStatusCode.NotFound);
            }
            return Ok(user_value);
        }

        //Return all pivot associated with a value a for user
        [HttpGet]
        [Route("/user/{email}/pivots/{value}")]
        public IActionResult get_pivots_for_value(string email, string value){
            var user = _context.Users
                .Where(u => u.email == email)
                .FirstOrDefault();
            
            if(user == null){
                return StatusCode((int)HttpStatusCode.NotFound);
            }

            var values = _context.UserValues
                .Where(u => u.User == user)
                .Where(u => u.value == value)
                .Include(u => u.Pivot)
                //.Where(u => u.Pivot.restitution_enabled == true)
                .ToList();
            
            return Ok(values);
        }

        //Return all value for user
        [HttpGet]
        [Route("/user/{email}/pivots")]
        public IActionResult get_values_user(string email){
            var user = _context.Users
                .Where(u => u.email == email)
                .FirstOrDefault();
            
            if(user == null){
                return StatusCode((int)HttpStatusCode.NotFound);
            }

            var user_values = _context.UserValues
                .Where(u => u.User == user)
                .Include(u => u.Pivot)
                .Where(u => u.Pivot.restitution_enabled == true)
                .ToList();
            return Ok(user_values);
        }

        //Return all value as pivot:value Dict for user
        [HttpGet]
        [Route("/user/{email}/pivots_v1")]
        public IActionResult get_pivots_user_v1(string email){
            var user = _context.Users
                .Where(u => u.email == email)
                .FirstOrDefault();
            
            if(user == null){
                return StatusCode((int)HttpStatusCode.NotFound);
            }

            //Populate enabled pivots
            var user_values_enabled = _context.UserValues
                .Where(u => u.User == user)
                .Include(u => u.Pivot)
                .Where(u => u.Pivot.restitution_enabled == true)
                .ToList();

            Dictionary<string, string> pivot_value = new Dictionary<string, string>();

            foreach (var item in user_values_enabled)
            {
                pivot_value[item.Pivot.name] = item.value; 
            }

            //Populate disabled pivots with blank values
            var user_values_disabled = _context.UserValues
                .Where(u => u.User == user)
                .Include(u => u.Pivot)
                .Where(u => u.Pivot.restitution_enabled == false)
                .ToList();

            foreach (var item in user_values_disabled)
            {
                pivot_value[item.Pivot.name] = " "; 
            }

            string json = JsonConvert.SerializeObject(pivot_value, Formatting.Indented);

            return Ok(json);
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
                    .Where(uv => uv.User == user)
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
            user_value.weight = Convert.ToInt64(_iconfiguration["weigth_for_creation"]);

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
        //Forbid pivot creation
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

            //Retrieve the value corresponding to the pivot 
            var user_value = _context.UserValues
                .Where(u => u.Pivot == pivot)
                .Where(u => u.User == user)
                .FirstOrDefault();

            if(user_value == null){
                return BadRequest("This pivot does not exist for this user.");
            }

            user_value.value = item.Value;
            user_value.updated_at = DateTime.Now;

            _context.SaveChanges();
            return Ok(user_value);
        }

        [HttpDelete("{email}")]
        public IActionResult DeleteUser(string email)
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

        [HttpDelete("/value/{user_value_id}")]
        public IActionResult DeleteUserValue(long user_value_id)
        {
            var user_value = _context.UserValues
                .Where(u => u.userValueId == user_value_id)
                .FirstOrDefault();
            
            if(user_value == null ){
                return NotFound();
            }

            _context.UserValues.Remove(user_value);
            _context.SaveChanges();
            return new NoContentResult();
        }
    }
}