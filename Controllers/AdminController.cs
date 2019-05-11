using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Autofiller.Models;

namespace Application_WEB_MVC.Controllers
{
    [Route("/[controller]")]
    public class AdminController : Controller
    {

        private readonly AutofillerContext _context;
        private readonly IConfiguration _iconfiguration;
        private readonly ILogger _logger;

        public AdminController(AutofillerContext context,
                               IConfiguration iconfiguration,
                               ILogger<AdminController> logger)
        {
            _context = context;
            _iconfiguration = iconfiguration;
            _logger = logger;
        }

        //Create a new pivot
        [HttpPost]
        [Route("/admin/pivot/{namePivot}")]        
        public IActionResult createPivot(string namePivot)
        {
            _logger.LogWarning("\n\nLe nom du nouveau pivot est: " + namePivot);
            var oldPivot = _context.Pivots
                .Where(p => p.name == namePivot)
                .FirstOrDefault();
            
            //Pivot already exists
            if(oldPivot != null){
                return StatusCode((int)HttpStatusCode.Conflict);
            }

            Pivot newPivot = new Pivot();
            newPivot.name = namePivot;
            newPivot.created_at = DateTime.Now;
            newPivot.updated_at = DateTime.Now;
            _context.Add(newPivot);
            _context.SaveChanges();

            return Ok(newPivot);
        }

        //Init all pivots for system
        [HttpPost]
        [Route("/admin/pivot/init")]        
        public IActionResult initPivot(string namePivot)
        {
            //Get all pivot from config file
            var pivots = _iconfiguration.GetSection("pivots").Get<List<String>>();
            _logger.LogWarning(pivots.GetType().ToString());

            foreach(var pivot_name in pivots){
                _logger.LogInformation("Creating pivot: " + pivot_name);
                Pivot newPivot = new Pivot();
                newPivot.name = pivot_name;
                newPivot.created_at = DateTime.Now;
                newPivot.updated_at = DateTime.Now;
                newPivot.restitution_enabled = true;
                _context.Add(newPivot);
            }
            _context.SaveChanges();

            return Ok();
        }

        //Init All keys for creating a profil on dedicated special page 
        [HttpPost]
        [Route("/admin/create_profil/init_key")]
        public IActionResult initCreateProfilPage(){
            var domain_create_profil = _iconfiguration.GetSection("domain_create_profil").Get<String>();

            _logger.LogInformation("[initCreateProfilPage] domain_create_profil: " + domain_create_profil);

            //Getting proper website used for creating a profil 

            var website = _context.Websites
                .Where(k => k.domaine == domain_create_profil)
                .FirstOrDefault();

            if(website == null ){
                return NotFound("Could not found domaine create profil");
            }

            

            // Create each key for each pivot
            // ------------------------------
            
            
            // civility

            var pivot = _context.Pivots.Where(p => p.name == "civility").FirstOrDefault();
            var key = Utils.createKeyProfil(website, pivot, "civility__select");
            key.civility = 100;
            _context.Add(key);

            // email 
            pivot = _context.Pivots.Where(p => p.name == "main_email").FirstOrDefault();
            key = Utils.createKeyProfil(website, pivot, "email__text");
            key.main_email = 100;
            _context.Add(key);

            // first_name
            pivot = _context.Pivots.Where(p => p.name == "first_name").FirstOrDefault();
            key = Utils.createKeyProfil(website, pivot, "firstname__text");
            key.first_name = 100;
            _context.Add(key);

            // family_name 
            pivot = _context.Pivots.Where(p => p.name == "family_name").FirstOrDefault();
            key = Utils.createKeyProfil(website, pivot, "lastname__text");
            key.family_name = 100;
            _context.Add(key);

            // day of birth
            pivot = _context.Pivots.Where(p => p.name == "day_of_birth").FirstOrDefault();
            key = Utils.createKeyProfil(website, pivot, "day_birth__text");
            key.day_of_birth = 100;
            _context.Add(key);

            // month of birth
            pivot = _context.Pivots.Where(p => p.name == "month_of_birth").FirstOrDefault();
            key = Utils.createKeyProfil(website, pivot, "month_birth__text");
            key.month_of_birth = 100;
            _context.Add(key);

            // year of birth
            pivot = _context.Pivots.Where(p => p.name == "year_of_birth").FirstOrDefault();
            key = Utils.createKeyProfil(website, pivot, "year_birth__text");
            key.year_of_birth = 100;
            _context.Add(key);

            // indicative
            pivot = _context.Pivots.Where(p => p.name == "indicative").FirstOrDefault();
            key = Utils.createKeyProfil(website, pivot, "indicative__text");
            key.indicative = 100;
            _context.Add(key);

            // cellphone
            pivot = _context.Pivots.Where(p => p.name == "cellphone_number").FirstOrDefault();
            key = Utils.createKeyProfil(website, pivot, "cellphone__text");
            key.cellphone_number = 100;
            _context.Add(key);

            // homephone
            pivot = _context.Pivots.Where(p => p.name == "homephone").FirstOrDefault();
            key = Utils.createKeyProfil(website, pivot, "homephone__text");
            key.homephone = 100;
            _context.Add(key);

            // address
            pivot = _context.Pivots.Where(p => p.name == "address").FirstOrDefault();
            key = Utils.createKeyProfil(website, pivot, "street_adress__text");
            key.address = 100;
            _context.Add(key);

            // postal code
            pivot = _context.Pivots.Where(p => p.name == "postal_code").FirstOrDefault();
            key = Utils.createKeyProfil(website, pivot, "postal_code__text");
            key.postal_code = 100;
            _context.Add(key);

            // city
            pivot = _context.Pivots.Where(p => p.name == "home_city").FirstOrDefault();
            key = Utils.createKeyProfil(website, pivot, "city__text");
            key.home_city = 100;
            _context.Add(key);

            // country
            pivot = _context.Pivots.Where(p => p.name == "country").FirstOrDefault();
            key = Utils.createKeyProfil(website, pivot, "country__text");
            key.country = 100;
            _context.Add(key);

            _context.SaveChanges();

            return Ok();            
        }

        //Return values in conf
        [HttpGet]
        [Route("/admin/test_conf")]

        public IActionResult test_conf(){
            var connectionString = _iconfiguration["ConnectionString"];  
            return Ok(connectionString);
        }
    }
}