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
                _context.Add(newPivot);
            }
            //_context.SaveChanges();

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