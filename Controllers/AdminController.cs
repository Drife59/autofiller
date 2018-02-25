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
    [Route("/[controller]")]
    public class AdminController : Controller
    {

        private readonly AutofillerContext _context;

        public AdminController(AutofillerContext context)
        {
            _context = context;
        }

        //Replace a pivot in db TODO
        [HttpPost]
        [Route("/admin/pivot/apply_merge/{pivot_origine}/{pivot_remplacement}")]
        public IActionResult remplacement_direct(int pivot_origine_id, int pivot_remplacement_id){

            var pivot_orig = _context.Pivots
                .Where(p => p.pivotId == pivot_origine_id)
                .FirstOrDefault();

            var pivot_remp = _context.Pivots
                .Where(p => p.pivotId == pivot_remplacement_id)
                .FirstOrDefault();

            var user_value = _context.UserValues
                .Where(uv => uv.Pivot == pivot_orig)
                .ToList();

            foreach(UserValue uv in user_value){
                uv.Pivot = pivot_remp;
                _context.Add(uv);
            }

            var keys = _context.Keys
                .Where(k => k.Pivot == pivot_orig)
                .ToList();

            foreach(Key k in keys){
                k.Pivot = pivot_remp;
                _context.Add(k);
            }
            _context.SaveChanges();
            
            return Ok("remplacement de " + pivot_orig.name + " par " + pivot_remp.name);
        }

        //Record a new merge request
        [HttpPost]
        [Route("/admin/pivot/remplacement_async/{pivot_origine}/{pivot_remplacement}")]
        public IActionResult remplacement_async(string pivot_origine, string pivot_remplacement,
                                                [FromBody] MergeRequest merge_request){
            
            if (merge_request == null || merge_request.email   == null || merge_request.domaine == null
                                      || merge_request.cle_req == null || merge_request.valeur  == null)
            {
                return BadRequest("You need to give email, domaine, cle_req & valeur for merge request");
            }

            //Load Data from DB to create a "high level" merge request
            var merge = new Merge();
            merge.created_at = DateTime.Now;

            var user = _context.Users
                .Where(u => u.email == merge_request.email) 
                .FirstOrDefault();

            merge.User = user;

            var website = _context.Websites
                .Where(w => w.domaine == merge_request.domaine)
                .FirstOrDefault();

            merge.Website = website;

            var pivot_orig = _context.Pivots
                .Where(p => p.name == pivot_origine)
                .FirstOrDefault();

            merge.PivotFromUser = pivot_orig;

            var pivot_remp = _context.Pivots
                .Where(p => p.name == pivot_remplacement)
                .FirstOrDefault();
            
            merge.PivotFromWebsite = pivot_remp;

            _context.Add(merge);
            _context.SaveChanges(); 
            return Ok(merge);      
        }

        //Return all merge pending
        [HttpGet]
        [Route("/admin/merges/{nombre}")]
        public IActionResult pivot_attente(int nombre){
            return Ok(_context.Merges.Take(nombre));
        }
    }
}