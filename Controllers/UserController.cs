using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

//Import de nos modèles dédiés
using Website.Models;

namespace Application_WEB_MVC.Controllers
{
    [Route("/[controller]")]
    public class UserController : Controller
    {
        //parametre yahou passable en argument ou dans l'url
        /*[Route("{yahou?}")]
        public string Index(int? yahou)
        {
            return "Koukou " + yahou;
        }*/

        [HttpPost]
        [Route("/user/{courriel}")]
        public IActionResult new_user(string courriel){
            //Cas du conflict
            //return StatusCode((int)HttpStatusCode.Conflict);
            
            //Tout se passe bien, 200OK 
            return Ok("haha koukoukoukou");
            //return StatusCode(200, "Ha que koukou !");
        }

        [HttpGet]
        [Route("/user/{courriel}/pivots")]
        public IActionResult get_pivots_user(string courriel){
            //Utilisateur non connu
            return StatusCode((int)HttpStatusCode.NotFound);
            
            //Tout se passe bien, 200OK 
            //return Ok("haha koukoukoukou");
            //return StatusCode(200, "Ha que koukou !");
        }

        /*Il est important de différencier la création d'une nouvelle clé de sa
        mise à jour. Ces dernières ne déclencherons pas le même algo.
        POST = création, PUT = mise à jour
        */

        [HttpPost]
        [Route("/user/{courriel}/pivots")]
        public IActionResult add_pivot_user(string courriel, [FromBody] PivotUserRequest item)
        {
            if (item == null || item.Pivot == null || item.Value == null)
            {
                return BadRequest("You need to give pivot & value for user");
            }

            //Utilisateur non connu
            //return StatusCode((int)HttpStatusCode.NotFound);

            //Everything Ok :)
            return Ok("Création du pivot " + item.Pivot + " avec la valeur " + item.Value + " pour le user " + courriel);
        }

        [HttpPut]
        [Route("/user/{courriel}/pivots")]
        public IActionResult maj_pivot_user(string courriel, [FromBody] PivotUserRequest item)
        {
            if (item == null || item.Pivot == null || item.Value == null)
            {
                return BadRequest("You need to give pivot & value for user");
            }

            //Utilisateur non connu
            //return StatusCode((int)HttpStatusCode.NotFound);

            //Everything Ok :)
            return Ok("Maj du pivot " + item.Pivot + " avec la valeur " + item.Value + " pour le user " + courriel);
        }

        [HttpDelete]
        [Route("/user/{courriel}")]
        public IActionResult delete_user(string courriel){
            //Utilisateur non connu
            //return StatusCode((int)HttpStatusCode.NotFound);
            
            //Tout se passe bien, 204 OK 
            return new NoContentResult();
        }
    }
}