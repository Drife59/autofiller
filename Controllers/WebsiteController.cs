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

    //Test avec personnalisation maximale d'adresse
    //[Route("koukou/[controller]/[action]")]

    //Laisse volontairement le paramètre action à personnaliser par chaque fonction
    [Route("/[controller]/")]
    public class WebsiteController : Controller
    {

        private readonly AutofillerContext _context;

        public WebsiteController(AutofillerContext context)
        {
            _context = context;
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

        //Post: création d'un nouveau pivot
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

            var pivot = new Pivot();
            pivot.name = item.Pivot;
            pivot.created_at = DateTime.Now;
            pivot.updated_at = DateTime.Now;
            _context.Add(pivot);

            var key = new Key();
            key.code = item.Cle;
            key.Pivot = pivot;
            key.created_at = DateTime.Now;
            key.updated_at = DateTime.Now;
            key.Website = website;
            _context.Add(key);

            _context.SaveChanges();
            return Ok(key);
        }

        //Put: mise à jour d'un pivot associé à une clée
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
            var keys = _context.Keys
                .Where(k => k.Website == website)
                .ToList();

            return Ok(keys);
        }

        [HttpDelete("{url_domaine}")]
        public IActionResult Delete(string url_domaine)
        {
            return new NoContentResult();
        }
    }
}