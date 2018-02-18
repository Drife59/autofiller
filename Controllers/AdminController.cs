using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

//Import de nos modèles dédiés
using Autofiller.Models;

namespace Application_WEB_MVC.Controllers
{
    [Route("/[controller]")]
    public class AdminController : Controller
    {

        private readonly AutofillerContext _context;

        public AdminController(AutofillerContext context)
        {
            _context = context;
        }

        //Remplace directement base un pivot par un autre
        [HttpPost]
        [Route("/admin/pivot/remplacement_direct/{pivot_origine}/{pivot_remplacement}")]
        public IActionResult remplacement_direct(string pivot_origine, string pivot_remplacement){

            //A coder côté back ?
            //deduplication_volee_direct(pivot_origine, pivot_remplacement)
            
            //Tout se passe bien, 200OK 
            return Ok("remplacement de " + pivot_origine + " par " + pivot_remplacement);
        }

        //Enregistre une nouvelle demande de merge
        [HttpPost]
        [Route("/admin/pivot/remplacement_async/{pivot_origine}/{pivot_remplacement}")]
        public IActionResult remplacement_async(string pivot_origine, string pivot_remplacement,
                                                [FromBody] MergeRequest merge_request){
            
            if (merge_request == null || merge_request.email   == null || merge_request.domaine == null
                                      || merge_request.cle_req == null || merge_request.valeur  == null)
            {
                return BadRequest("You need to give email, domaine, cle_req & valeur for merge request");
            }


            //TODO algo à faire de préparation de merge:
            //# 2) On prend un domaine comportant le pivot de remplacement
            //# On récupère la clé qui correspond

            //Tout se passe bien, 200OK 
            return Ok("Merge de " + pivot_origine + " par " + pivot_remplacement + 
                      " pour " + merge_request.email + " depuis " + merge_request.domaine);
        }

        //Retour de tous les merges en attente de remplacement
        [HttpGet]
        [Route("/admin/pivot/attente/{nombre}")]
        public IActionResult pivot_attente(int nombre){

            //Tout se passe bien, 200OK 
            return Ok("Retour de " + nombre + " merges.");
        }
    }
}