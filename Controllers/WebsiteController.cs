using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
            //Retour pour un domaine non trouvé
            //return NotFound();

            //retour pour un domaine trouvé
            return Ok();
        }

        [HttpPost]
        [Route("{url_domaine}")]        
        public IActionResult New_Domaine(string url_domaine)
        {

            Website new_website = new Website();
            new_website.domaine = url_domaine;
            new_website.created_at = new DateTime();
            new_website.updated_at = new DateTime();
            _context.Add(new_website);
            _context.SaveChanges();

            //return StatusCode((int)HttpStatusCode.Conflict);
            return Ok(new_website);
            //return "Création de " + url_domaine;
        }

        [HttpGet]
        [Route("{url_domaine}/pivots")]        
        public string Get_cles(string url_domaine)
        {
            return "Récupération des pivots de " + url_domaine;
        }

        //TODO: maj front
        //Post: création d'un nouveau pivot
        [HttpPost]
        [Route("{url_domaine}/pivot")]        
        public string New_pivot(string url_domaine, [FromBody] PivotDomaineRequest item)
        {
            /* 
            if (item == null)
            {
                return BadRequest();
            }
            */
            return "Création du pivot " + item.Pivot + " pour la clé " + item.Cle + " dans le domaine " + url_domaine;
        }

        //Put: mise à jour d'un pivot associé à une clée
        [HttpPut]
        [Route("{url_domaine}/pivot")]        
        public string Maj_pivot(string url_domaine, [FromBody] PivotDomaineRequest item)
        {
            return "Maj du pivot " + item.Pivot + " pour la clé " + item.Cle + " dans le domaine " + url_domaine;
        }

        [HttpDelete("{url_domaine}")]
        public IActionResult Delete(string url_domaine)
        {
            return new NoContentResult();
        }
    }
}