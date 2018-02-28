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
        [Route("/admin/merges/apply/{merge_id}/{pivot_to_keep_id}")]
        public IActionResult remplacement_direct(int merge_id, int pivot_to_keep_id){

            var merge = _context.Merges
                .Where(m => m.mergeId == merge_id)
                .Include(m => m.PivotFromUser)
                .Include(m => m.PivotFromWebsite)
                .FirstOrDefault();

            if( merge == null){
                return NotFound("Merge does not exist");
            }

            var pivot_to_keep = _context.Pivots
                .Where(p => p.pivotId == pivot_to_keep_id)
                .FirstOrDefault();

            if( pivot_to_keep == null){
                return BadRequest("Pivot to keep does not exist");
            }

            var pivot_to_replace = merge.PivotFromUser;
            if( merge.PivotFromUser == pivot_to_keep ){
                pivot_to_replace = merge.PivotFromWebsite;
            }

            if( pivot_to_replace == null){
                return BadRequest("Pivot to use for replacement does not exist");
            }

            //Get user value to be modified
            var user_value = _context.UserValues
                .Where(uv => uv.Pivot == pivot_to_replace)
                .ToList();

            foreach(UserValue uv in user_value){
                uv.Pivot = pivot_to_keep;
                //_context.Add(uv);
            }

            //Get keys to be modified
            var keys = _context.Keys
                .Where(k => k.Pivot == pivot_to_replace)
                .ToList();

            foreach(Key k in keys){
                k.Pivot = pivot_to_keep;
                //_context.Add(k);
            }
            _context.SaveChanges();
            
            return Ok("remplacement de " + pivot_to_replace.name + " par " + pivot_to_keep.name);
        }

        //Record a new merge request
        [HttpPost]
        [Route("/admin/merge/{pivot_origine}/{pivot_remplacement}")]
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

            if( user == null){
                return BadRequest("User cannot be found");
            }

            merge.User = user;

            var website = _context.Websites
                .Where(w => w.domaine == merge_request.domaine)
                .FirstOrDefault();

            if( website == null){
                return BadRequest("Website cannot be found");
            }

            merge.Website = website;

            var pivot_orig = _context.Pivots
                .Where(p => p.name == pivot_origine)
                .FirstOrDefault();
            
            if( pivot_orig == null){
                return BadRequest("Origin pivot cannot be found");
            }

            merge.PivotFromUser = pivot_orig;

            var pivot_remp = _context.Pivots
                .Where(p => p.name == pivot_remplacement)
                .FirstOrDefault();

            if( pivot_remp == null){
                return BadRequest("Remplacement pivot cannot be found");
            }
            
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