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

        //return all keys for website
        [HttpGet]
        [Route("{url_domaine}/pivots")]        
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

        //Compatibility method for front v1, return Dict Key:Value<pivot>
        [HttpGet]
        [Route("{url_domaine}/pivots_v1")]        
        public IActionResult Get_cles_v1(string url_domaine)
        {
            var website = _context.Websites
                .Where(w => w.domaine == url_domaine)
                .FirstOrDefault();

            if(website == null){
                return NotFound();
            }
            
            var keys = _context.Keys
                .Where(k => k.Website == website)
                .Include(k => k.Pivot)
                .ToList();
            
            Dictionary<string, string> cle_pivot = new Dictionary<string, string>();

            foreach (var item in keys)
            {
                cle_pivot[item.code] = item.Pivot.name; 
            }

            string json = JsonConvert.SerializeObject(cle_pivot, Formatting.Indented);
            return Ok(json);
        }

                    
        //Post: Create key and associate pivot.
        //Create pivot if totally new in DB
        //Force Key creation, don't allow update
        [HttpPost]
        [Route("{url_domaine}/pivot")]        
        public IActionResult New_pivot(string url_domaine, [FromBody] PivotDomaineRequest item)
        {

            if (item == null)
            {
                return BadRequest();
            }

            var website = _context.Websites
                .Where(w => w.domaine == url_domaine)
                .FirstOrDefault();
            
            if(website == null){
                return NotFound();
            }

            var key = _context.Keys
                .Where(k => k.code == item.Cle)
                .Where(k => k.Website == website)
                .FirstOrDefault();
            
            if( key != null ){
                return BadRequest("This key already exist for this Website");
            }

            var pivot = _context.Pivots
                .Where(p => p.name == item.Pivot) 
                .FirstOrDefault();

            //New pivot in all db, create it
            if( pivot == null){
                pivot = new Pivot();
                pivot.name = item.Pivot;
                pivot.created_at = DateTime.Now;
                _context.Add(pivot);
                _logger.LogInformation("WebsiteController: create for the first time pivot " + pivot.name);
            }
            pivot.updated_at = DateTime.Now;
            
            key = new Key();
            key.code = item.Cle;
            key.Pivot = pivot;
            key.created_at = DateTime.Now;
            key.updated_at = DateTime.Now;
            key.Website = website;
            _context.Add(key);

            _context.SaveChanges();
            return Ok(key);
        }

        // Put: Update pivot associated to key
        // Fail if key or Pivot does not exist
        // return pivot associated
        [HttpPut]
        [Route("{url_domaine}/pivot")]        
        public IActionResult Maj_pivot(string url_domaine, [FromBody] PivotDomaineRequest item)
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
                .Where(k => k.code == item.Cle)
                .FirstOrDefault();

            if(key == null ){
                return BadRequest("Key does not exist");
            }

            //Get pivot we want to set on Key
            var pivot = _context.Pivots
                .Where(p => p.name == item.Pivot)
                .FirstOrDefault();

            //it must exist, his value must have been retrieved
            if(pivot == null ){
                return BadRequest("Pivot does not exist");
            }

            key.Pivot = pivot;
            _context.SaveChanges();
            return Ok(pivot);
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