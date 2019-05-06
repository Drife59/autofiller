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

    //Test avec personnalisation maximale d'adresse
    //[Route("koukou/[controller]/[action]")]

    //Laisse volontairement le paramètre action à personnaliser par chaque fonction
    [Route("/[controller]/")]
    public class WebsiteController : Controller
    {

        private readonly AutofillerContext _context;
        private readonly IConfiguration _iconfiguration;
        //private readonly IWebsiteRepository _websiteRepository;
        private readonly ILogger _logger;

        public WebsiteController(AutofillerContext context, 
                                //IWebsiteRepository WebsiteRepository,
                                ILogger<WebsiteController> logger,
                                IConfiguration iconfiguration)
        {
            _context = context;
            _iconfiguration = iconfiguration;
            //_websiteRepository = WebsiteRepository;
            _logger = logger;
        }

        //parametre yahou passable en argument ou dans l'url
        [Route("{yahou?}")]
        public string Index(int? yahou)
        {
            return "Koukou " + yahou;
        }

        [HttpGet]
        [Route("{url_domaine}")]        
        public IActionResult Check_existence(string url_domaine)
        {
            var website = _context.Websites
                .Where(w => w.domaine == url_domaine) 
                .FirstOrDefault();
            
            if(website == null){
                return NotFound();
            }
            return Ok(website);
        }

        [HttpPost]
        [Route("{url_domaine}")]        
        public IActionResult New_Domaine(string url_domaine)
        {

            var website = _context.Websites
                .Where(w => w.domaine == url_domaine) 
                .FirstOrDefault();

            //Only add website if it does not already exist
            if( website == null){
                Website new_website = new Website();
                new_website.domaine = url_domaine;
                new_website.created_at = DateTime.Now;
                new_website.updated_at = DateTime.Now;
                _context.Add(new_website);
                _context.SaveChanges();              
                return Ok(new_website);
            }else{
                return StatusCode((int)HttpStatusCode.Conflict);
            }
        }
        
        //return a key for website, if found
        [HttpGet]
        [Route("{url_domaine}/key/{key_code}")]        
        public IActionResult Get_cles(string url_domaine, string key_code)
        {
            var website = _context.Websites
                .Where(w => w.domaine == url_domaine)
                .FirstOrDefault();

            if(website == null){
                return NotFound();
            }

            var key = _context.Keys
                .Where(k => k.code == key_code)
                .Where(k => k.Website == website)
                .Include(w => w.Pivot)
                .FirstOrDefault();
            
            if( key == null){
                return NotFound("Key for website could not be found.");
            }

            return Ok(key);
        }

        //V5 return all keys for website
        [HttpGet]
        [Route("{url_domaine}/keys")]        
        public IActionResult Get_cles(string url_domaine)
        {
            var website = _context.Websites
                .Where(w => w.domaine == url_domaine)
                .Include(w => w.Keys)
                .ThenInclude(w => w.Pivot)
                .FirstOrDefault();
            
            if(website == null){
                return NotFound();
            }
            return Ok(website.Keys);
        }
                          
        //Post: Create key
        //Force Key creation, don't allow update
        [HttpPost]
        [Route("{url_domaine}/key")]        
        public IActionResult createKey(string url_domaine, [FromBody] KeyRequest item)
        {

            if (item == null)
            {
                return BadRequest("Key was not in body request.");
            }

            var website = _context.Websites
                .Where(w => w.domaine == url_domaine)
                .FirstOrDefault();
            
            if(website == null){
                return NotFound();
            }

            var key = _context.Keys
                .Where(k => k.code == item.cle)
                .Where(k => k.Website == website)
                .FirstOrDefault();
            
            if( key != null ){
                return BadRequest("This key already exist for this Website");
            }

            key = new Key();
            //If pivot reference exist, associate it
            if(item.pivot_reference != "null" && item.pivot_reference != "undefined" && item.pivot_reference != ""){
                var pivot = _context.Pivots
                .Where(p => p.name == item.pivot_reference) 
                .FirstOrDefault();
                key.Pivot = pivot;
            }
            //No pivot reference
            else{
                key.Pivot = null;
            }
            
            key.code = item.cle;
            key.created_at = DateTime.Now;
            key.updated_at = DateTime.Now;
            key.Website = website;

            //Set all weight
            key.first_name        = Int32.Parse(item.first_name);
            key.family_name       = Int32.Parse(item.family_name);
            key.postal_code       = Int32.Parse(item.postal_code);
            key.home_city         = Int32.Parse(item.home_city);
            key.cellphone_number  = Int32.Parse(item.cellphone_number);
            key.main_email        = Int32.Parse(item.main_email);
            key.main_full_address = Int32.Parse(item.main_full_address);
            key.address           = Int32.Parse(item.address);

            key.day_of_birth      = Int32.Parse(item.day_of_birth);
            key.month_of_birth    = Int32.Parse(item.month_of_birth);
            key.year_of_birth     = Int32.Parse(item.year_of_birth);

            key.company         = Int32.Parse(item.company);
            key.homephone       = Int32.Parse(item.homephone);
            /*key.cvv             = Int32.Parse(item.cvv);
            key.cardexpirymonth = Int32.Parse(item.cardexpirymonth);
            key.cardexpiryyear  = Int32.Parse(item.cardexpiryyear);
            */
            key.full_birthdate = Int32.Parse(item.full_birthdate);
            key.country        = Int32.Parse(item.country);


            _context.Add(key);

            _context.SaveChanges();
            return Ok(key);
        }


        //Update Key
        [HttpPut]
        [Route("{url_domaine}/key")]        
        public IActionResult updateKey(string url_domaine, [FromBody] KeyRequest item)
        {

            var website = _context.Websites
                .Where(w => w.domaine == url_domaine)
                .FirstOrDefault();
            
            if(website == null ){
                return NotFound();
            }

            //Key must exist, we are in PUT method
            var key = _context.Keys
                .Where(k => k.Website == website)
                .Where(k => k.code == item.cle)
                .FirstOrDefault();

            if(key == null ){
                return NotFound();
            }

            //If pivot reference exist, associate it
            if(item.pivot_reference != "null" && item.pivot_reference != "undefined" && item.pivot_reference != ""){
                var pivot = _context.Pivots
                .Where(p => p.name == item.pivot_reference) 
                .FirstOrDefault();
                key.Pivot = pivot;
            }
            //No pivot reference
            else{
                key.Pivot = null;
            }

            key.updated_at = DateTime.Now;

            //Update all weight
            key.first_name        = Int32.Parse(item.first_name);
            key.family_name       = Int32.Parse(item.family_name);
            key.postal_code       = Int32.Parse(item.postal_code);
            key.home_city         = Int32.Parse(item.home_city);
            key.cellphone_number  = Int32.Parse(item.cellphone_number);
            key.main_email        = Int32.Parse(item.main_email);
            key.main_full_address = Int32.Parse(item.main_full_address);
            key.address           = Int32.Parse(item.address);
            key.day_of_birth      = Int32.Parse(item.day_of_birth);
            key.month_of_birth    = Int32.Parse(item.month_of_birth);
            key.year_of_birth     = Int32.Parse(item.year_of_birth);

            key.company         = Int32.Parse(item.company);
            key.homephone       = Int32.Parse(item.homephone);
            /* key.cvv             = Int32.Parse(item.cvv);
            key.cardexpirymonth = Int32.Parse(item.cardexpirymonth);
            key.cardexpiryyear  = Int32.Parse(item.cardexpiryyear);
            */
            key.full_birthdate = Int32.Parse(item.full_birthdate);
            key.country        = Int32.Parse(item.country);


            _context.SaveChanges();
            return Ok(key);
        }

        //Delete Domain and associated keys
        [HttpDelete("{url_domaine}")]
        public IActionResult Delete(string url_domaine)
        {
            var website = _context.Websites
                .Where(w => w.domaine == url_domaine)
                .FirstOrDefault();
            
            if(website == null ){
                return NotFound();
            }

            _context.Websites.Remove(website);
            _context.Keys.RemoveRange(_context.Keys.Where(k => k.Website == website));
            _context.SaveChanges();

            return new NoContentResult();
        }
    }
}