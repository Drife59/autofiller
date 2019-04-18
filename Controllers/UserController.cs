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

//for hash function
using System.Text;  
using System.Security.Cryptography;  


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

        //As of now, we only need Hash for password, that is why we put it here
        //Later on, we should create a package utils
        static string ComputeSha256Hash(string rawData)  
        {  
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())  
            {  
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));  
  
                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();  
                for (int i = 0; i < bytes.Length; i++)  
                {  
                    builder.Append(bytes[i].ToString("x2"));  
                }  
                return builder.ToString();  
            }  
        }  


        /*
            -------------
            PURE USER API
            -------------
         */

        //Allow connection to account
        // Check if user / pwd couple is good.
        [HttpGet]
        [Route("/user/{email}/{password}")]
        public IActionResult connection_user(string email, string password){

            var user = _context.Users
                .Where(u => u.email == email) 
                .FirstOrDefault();

            if( user == null){
                return NotFound("Cannot find user");
            }

            var password_hash = ComputeSha256Hash(password);

            if( user.password_hash != password_hash){
                return StatusCode((int)System.Net.HttpStatusCode.Forbidden, "Wrong password");
            }

            return Ok(user);
        }
        
        [HttpPost]
        [Route("/user/{email}/{password}")]
        public IActionResult new_user(string email, string password){

            var user = _context.Users
                .Where(u => u.email == email) 
                .FirstOrDefault();

            //Only add user if it does not already exist
            //Only store hashed password
            if( user == null){
                User new_user = new User();
                new_user.email = email;
                new_user.password_hash = ComputeSha256Hash(password);
                new_user.created_at = DateTime.Now;
                new_user.updated_at = DateTime.Now;
                _context.Add(new_user);
                _context.SaveChanges();              
                return Ok(new_user);
            }else{
                return StatusCode((int)HttpStatusCode.Conflict);
            }
        }

        //Reset a password
        [HttpPost]
        [Route("/password/{email}/{password}")]
        public IActionResult reset_password(string email, string password){

            var user = _context.Users
                .Where(u => u.email == email) 
                .FirstOrDefault();

            if( user == null){
                return NotFound("Cannot find user");
            }

            user.password_hash = ComputeSha256Hash(password);
            _context.SaveChanges();
            return Ok(user);
        }

        [HttpDelete]
        [Route("/user/{email}")]
        public IActionResult DeleteUser(string email)
        {
            var user = _context.Users
                .Where(u => u.email == email)
                .Include(u => u.UserValues)
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
            ----------------------------
            User value method, V5 Legacy
            ----------------------------
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
        //Filter on pivot is optionnal
        //filter_restitution_enabled is enabled by default, only get pivot with restitution_enabled = true
        //Set filter = false in query if you want values from disabled pivots
        [HttpGet]
        [Route("/user/{email}/values/v3")]
        public IActionResult get_values_user(string email, string pivot = null, Boolean filter_restitution_enabled = true){

            var user = _context.Users
                .Where(u => u.email == email)
                .FirstOrDefault();
            
            if(user == null){
                _logger.LogWarning("Cannot find user with email: " + email);
                return StatusCode((int)HttpStatusCode.NotFound);
            }

            if( pivot != null && filter_restitution_enabled == true){
                return Ok(_context.UserValues
                .Where(u => u.User == user)
                .Include(u => u.Pivot)
                .Where(u => u.Pivot.name == pivot)
                .Where(u => u.Pivot.restitution_enabled == true)
                .ToList());
            }else if (pivot != null){
                return Ok(_context.UserValues
                .Where(u => u.User == user)
                .Include(u => u.Pivot)
                .Where(u => u.Pivot.name == pivot)
                .ToList());
            }else if(filter_restitution_enabled == true){
                return Ok(_context.UserValues
                .Where(u => u.User == user)
                .Include(u => u.Pivot)
                .Where(u => u.Pivot.restitution_enabled == true)
                .ToList());
            }

            return  Ok(_context.UserValues
                .Where(u => u.User == user)
                .Include(u => u.Pivot)
                .ToList());
        }

        /*
        Return values without profil.
        V5 Legacy method.

        
        Return objects values as 
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
         */
        [HttpGet]
        [Route("/user/{email}/uservalue_profilless")]
        public IActionResult get_uservalue_profilless(string email){
            var user = _context.Users
                .Where(u => u.email == email)
                .FirstOrDefault();
            
            if(user == null){
                _logger.LogWarning("Cannot find user with email: " + email);
                return StatusCode((int)HttpStatusCode.NotFound);
            }

            var user_values_enabled = _context.UserValues
                .Where(u => u.User == user)
                .Where(u => u.Profil == null)
                .Include(u => u.Pivot)
                .Where(u => u.Pivot.restitution_enabled == true)
                .ToList();

            //21/03/2019: Now returning raw text as Chrome 73 does not allow CORS JSON request anymore
            return Ok(user_values_enabled);

            /* Dictionary<string, List<Dictionary<string,string>>> pivots_values = 
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
            */
        }


        //Add a new value line for a pivot for a user, with or without a profil
        //This function is compatible V5/V6, within an optional parameter profil_id
        [HttpPost]
        [Route("/user/{email}/pivot/{pivot_name}/value/{value_text}/{profil_id?}")]
        public IActionResult add_value_for_pivot(string email, string pivot_name, string value_text, int profil_id=-1)
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
                return NotFound("Cannot find pivot with name: " + pivot_name);
            }

            Profil profil = null;
            //Try to find profil: profil_id was provided

            if(profil_id != -1){
                _logger.LogInformation("Required adding value for the profil: " + profil_id);
                profil = _context.Profils
                .Where(p => p.profilId == profil_id)
                .FirstOrDefault();

                //We required a profil and did not find it
                if(profil == null){
                    return NotFound("Cannot add value: cannot find profil " + profil_id);
                }
            }else{
                _logger.LogInformation("Required adding value without profil.");
            }

            //Create the new line value for this pivot
            var user_value = new UserValue();
            user_value.value = value_text;
            user_value.Profil = profil;
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
        [Route("/value/{user_value_id}/weight/{weight}")]
        public IActionResult update_value_weigth(long user_value_id, decimal weight)
        {
            var user_value = _context.UserValues
                .Where(u => u.userValueId == user_value_id)
                .FirstOrDefault();

            if( user_value == null){
                return NotFound();
            }

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

        /*
            --------------
            Profil method
            --------------
        */

        //Create a new profile
        [HttpPost]
        [Route("/user/{email}/profil/{profilName}")]
        public IActionResult new_profil(string email, string profilName){

            var user = _context.Users
                .Where(u => u.email == email)
                .FirstOrDefault();
            
            if(user == null){
                _logger.LogWarning("Cannot find user with email: " + email);
                return StatusCode((int)HttpStatusCode.NotFound);
            }

            var profil = _context.Profils
                .Where(p => p.User == user)
                .Where(p => p.profilName == profilName) 
                .FirstOrDefault();

            //Only add profil if it does not already exist for this user
            if( profil == null){
                Profil new_profil = new Profil();
                new_profil.User = user;
                new_profil.profilName = profilName;
                new_profil.created_at = DateTime.Now;
                new_profil.updated_at = DateTime.Now;
                new_profil.weight = 1;
                _context.Add(new_profil);
                _context.SaveChanges();              
                return Ok(new_profil);
            }else{
                return StatusCode((int)HttpStatusCode.Conflict, "A profil with name " + profilName + " already exist");
            }
        }

        //Get all profil for a user
        [HttpGet("/user/{email}/profils")]
        public IActionResult GetProfils(string email){
            var user = _context.Users
                .Where(u => u.email == email)
                .FirstOrDefault();
            
            if(user == null){
                _logger.LogWarning("Cannot find user with email: " + email);
                return StatusCode((int)HttpStatusCode.NotFound);
            }
            return Ok(_context.Profils
                        .Where(p => p.User == user)
                        .ToList());
        }

        //Update a profil weight 
        [HttpPut("/user/{email}/profil/{profilId}/weight/{weight}")]
        public IActionResult updateProfilWeight(string email, int profilId, decimal weight){
            var user = _context.Users
                .Where(u => u.email == email)
                .FirstOrDefault();
            
            if(user == null){
                _logger.LogWarning("Cannot find user with email: " + email);
                return StatusCode((int)HttpStatusCode.NotFound, ("Cannot find user with email: " + email));
            }

            //Does not allow to modify a profil if it is not one of the user provided
            var profil = _context.Profils
                        .Where(p => p.User == user)
                        .Where(p => p.profilId == profilId)
                        .FirstOrDefault();

            profil.weight = weight;
            _context.SaveChanges();              

            return Ok(profil);
        }

        //Delete a profile
        [HttpDelete("/user/{email}/profil/{profilId}")]
        public IActionResult DeleteProfil(string email, long profilId)
        {
            var user = _context.Users
                .Where(u => u.email == email)
                .FirstOrDefault();
            
            if(user == null){
                _logger.LogWarning("Cannot find user with email: " + email);
                return StatusCode((int)HttpStatusCode.NotFound);
            }

            var profil = _context.Profils
                .Where(p => p.profilId == profilId)
                .FirstOrDefault();
            
            if(profil == null ){
                _logger.LogWarning("Cannot find profil with id: " + profilId);
                return NotFound();
            }

            if(profil.User != user){
                _logger.LogWarning("Profil " + profilId + " does not belong to user" + email);
                return StatusCode((int)System.Net.HttpStatusCode.Forbidden, "You are not allowed to modify this profil");
            }

            //First delete related user value
            var user_values = _context.UserValues
                .Where(u => u.Profil == profil)
                .ToList();

            _context.UserValues.RemoveRange(user_values);

            _context.Profils.Remove(profil);
            _context.SaveChanges();
            return new NoContentResult();
        }

        //Return all values for user bind with a profile.
        [HttpGet]
        [Route("/user/{email}/values_with_profil")]
        public IActionResult get_values_with_profil(string email){

            var user = _context.Users
                .Where(u => u.email == email)
                .FirstOrDefault();
            
            if(user == null){
                _logger.LogWarning("Cannot find user with email: " + email);
                return StatusCode((int)HttpStatusCode.NotFound);
            }

            return  Ok(_context.UserValues
                .Where(v => v.User == user)
                .Where(v => v.Profil != null)
                .Include(v => v.Profil)
                .Include(v => v.Pivot)
                .ToList());
        }
    }
}