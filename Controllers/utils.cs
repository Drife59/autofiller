using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using Autofiller.Models;


namespace Application_WEB_MVC.Controllers
{
    public static class Utils{

        //factorise all init we need to create a key
        public static Key createKeyProfil(Website website,Pivot pivot, string code_key){
            
            var key = new Key();

            key.Pivot = pivot;

            key.code = code_key;
            key.created_at = DateTime.Now;
            key.updated_at = DateTime.Now;
            key.Website = website;

            //Set all weight
            key.main_email        = 0;
            key.civility          = 0;
            key.first_name        = 0;
            key.family_name       = 0;
            key.postal_code       = 0;
            key.home_city         = 0;
            
            key.indicative              = 0;
            key.cellphone_number        = 0;
            key.short_cellphone_number  = 0;
            key.full_cellphone_number   = 0;

            
            key.main_full_address = 0;
            key.address           = 0;

            key.day_of_birth      = 0;
            key.month_of_birth    = 0;
            key.year_of_birth     = 0;

            key.company         = 0;
            key.homephone       = 0;
            key.full_birthdate = 0;
            key.country        = 0;

            return key;
        }
    }
}