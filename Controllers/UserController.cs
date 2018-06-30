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


        /*
            -------------
            PURE USER API
            -------------
         */
        
        [HttpPost]
        [Route("/user/{email}")]
        public IActionResult new_user(string email){

            var user = _context.Users
                .Where(u => u.email == email) 
                .FirstOrDefault();

            //Only add user if it does not already exist
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

        [HttpDelete("{email}")]
        public IActionResult DeleteUser(string email)
        {
            var user = _context.Users
                .Where(u => u.email == email)
                .FirstOrDefault();
            
            if(user == null ){
                _logger.LogWarning("Cannot find user with email: " + email);
                return NotFound();
            }

            _context.Users.Remove(user);
            _context.SaveChanges();
            return new NoContentResult();
        }

        //Return User if found, else null
        private User get_user_by_email(string email){
            //Get user then pivot for the line to be added
            var user = _context.Users
                .Where(u => u.email == email) 
                .FirstOrDefault();

            if(user == null){
                _logger.LogWarning("Cannot find user with email: " + email);
            }
            return user;
        }

        /*
            -----------------------
            V1 compatibility method
            -----------------------
        */

        //Return all values as pivot:value Dict for user
        [HttpGet]
        [Route("/user/{email}/pivots_v1")]
        public IActionResult get_pivots_user_v1(string email){
            var user = _context.Users
                .Where(u => u.email == email)
                .FirstOrDefault();
            
            if(user == null){
                _logger.LogWarning("Cannot find user with email: " + email);
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

        /*
            -----------------
            User value method
            -----------------
         */

        //Return all values associated with corresponding value text, among all pivots for a user
        //TODO: make dynamic filter on restitution_enabled option
        [HttpGet]
        [Route("/user/{email}/values/{value}")]
        public IActionResult get_all_values_for_text(string email, string value){
            var user = _context.Users
                .Where(u => u.email == email)
                .FirstOrDefault();
            
            if(user == null){
                _logger.LogWarning("Cannot find user with email: " + email);
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

        //Return all values for user
        [HttpGet]
        [Route("/user/{email}/values")]
        public IActionResult get_values_user(string email){
            var user = _context.Users
                .Where(u => u.email == email)
                .FirstOrDefault();
            
            if(user == null){
                _logger.LogWarning("Cannot find user with email: " + email);
                return StatusCode((int)HttpStatusCode.NotFound);
            }

            var user_values = _context.UserValues
                .Where(u => u.User == user)
                .Include(u => u.Pivot)
                .Where(u => u.Pivot.restitution_enabled == true)
                .ToList();
            return Ok(user_values);
        }

    
        /*Return objects values as 
            {
                pivot_name1: [
                    { uservalue_id, value_text, weigth},
                    ...
                    { uservalue_id, value_text, weigth}
                ],
                ...
                pivot_namen: [
                    { uservalue_id, value_text, weigth},
                    ...
                    { uservalue_id, value_text, weigth}
                ],
            }
            Required for frond db population.
         */
        [HttpGet]
        [Route("/user/{email}/pivots_v3")]
        public IActionResult get_pivots_user_v3(string email){
            var user = _context.Users
                .Where(u => u.email == email)
                .FirstOrDefault();
            
            if(user == null){
                _logger.LogWarning("Cannot find user with email: " + email);
                return StatusCode((int)HttpStatusCode.NotFound);
            }

            var user_values_enabled = _context.UserValues
                .Where(u => u.User == user)
                .Include(u => u.Pivot)
                .Where(u => u.Pivot.restitution_enabled == true)
                .ToList();

            Dictionary<string, List<Dictionary<string,string>>> pivots_values = 
                new Dictionary<string, List<Dictionary<string, string>>>();

            foreach (var user_value in user_values_enabled)
            {
                Dictionary<string, string> value_in_list = new Dictionary<string, string>();
                value_in_list["uservalue_id"] = user_value.userValueId.ToString();
                value_in_list["value_text"]   = user_value.value;
                value_in_list["weigth"]       = user_value.weight.ToString();

                //Key for main object
                var pivot_id = user_value.Pivot.name;
                
                if(pivots_values.ContainsKey(pivot_id)){
                    pivots_values[pivot_id].Add(value_in_list);
                }
                //If first time create pivot id key and list
                else{
                    pivots_values[pivot_id] = new List<Dictionary<string, string>>();
                    pivots_values[pivot_id].Add(value_in_list);
                }
            }

            string json = JsonConvert.SerializeObject(pivots_values, Formatting.Indented);
            return Ok(json);
        }

        //Post: Create a new value for a user, associated with a pivot
        //Create new pivot if needed
        // #OBSOLETE with new multivaluation
        [HttpPost]
        [Route("{email}/pivot")]        
        public IActionResult New_pivot_user(string email, [FromBody] PivotUserRequest item)
        {
            if (item == null || item.Pivot == null || item.Value == null)
            {
                _logger.LogError("You need to give pivot & value for user");
                return BadRequest("You need to give pivot & value for user");
            }

            var user = _context.Users
                .Where(u => u.email == email)
                .FirstOrDefault();

            if(user == null){
                _logger.LogWarning("Cannor find user with email: " + email);
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
                    _logger.LogError("This pivot already exist for this user");
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

        //Add a new value line for a pivot for a user
        [HttpPost]
        [Route("/user/{email}/pivot/{pivot_name}/value/{value_text}")]
        public IActionResult add_value_for_pivot(string email, string pivot_name, string value_text)
        {
            //Get user then pivot for the line to be added
            var user = get_user_by_email(email);
            if(user == null){
                return NotFound();
            }

            var pivot = _context.Pivots
                .Where(p => p.name == pivot_name) 
                .FirstOrDefault();

            if(pivot == null){
                _logger.LogWarning("Cannot find pivot with name: " + pivot_name);
                return NotFound();
            }

            //Create the new line value for this pivot
            var user_value = new UserValue();
            user_value.value = value_text;
            user_value.created_at = DateTime.Now;
            user_value.updated_at = DateTime.Now;
            user_value.User = user;
            user_value.Pivot = pivot;
            user_value.weight = Convert.ToDecimal(_iconfiguration["weigth_for_creation"]);

            _context.Add(user_value);
            _context.SaveChanges();

            return Ok(user_value);
        }

        //Update weight for user value
        [HttpPut]
        [Route("/value/{user_value_id}/poid/{weight}")]
        public IActionResult update_value_weigth(long user_value_id, long weight)
        {
            var user_value = _context.UserValues
                .Where(u => u.userValueId == user_value_id)
                .FirstOrDefault();

            user_value.weight = weight;
            _context.SaveChanges();

            return Ok(user_value);
        }

        [HttpDelete("/value/{user_value_id}")]
        public IActionResult DeleteUserValue(long user_value_id)
        {
            var user_value = _context.UserValues
                .Where(u => u.userValueId == user_value_id)
                .FirstOrDefault();
            
            if(user_value == null ){
                _logger.LogWarning("Cannot find user with id: " + user_value_id);
                return NotFound();
            }

            _context.UserValues.Remove(user_value);
            _context.SaveChanges();
            return new NoContentResult();
        }
    }
}