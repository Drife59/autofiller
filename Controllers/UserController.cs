using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
//Nécessaire pour IHttpActionResult
//using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Application_WEB_MVC.Controllers
{
    [Route("/[controller]")]
    public class UserController : Controller
    {
        //parametre yahou passable en argument ou dans l'url
        [Route("{yahou?}")]
        public string Index(int? yahou)
        {
            return "Koukou " + yahou;
        }

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



    }

    /*
    @app.route('/user/<courriel>/pivot', methods=['POST'])
    def add_pivot_user(courriel):
        """Création d'un nouveau pivot pour le user."""

        user = None
        try:
            user = User.objects.get(email=courriel)
        except DoesNotExist:
            abort(404, "L'utilisateur n'existe pas.")

        #Validation du corps de la requête
        nom_pivot = None
        try:
            nom_pivot = request.get_json()['nom_pivot']
        except Exception as e:
            abort(400, "Erreur lors de la récupération de la variable "
                       "'nom_pivot': %s" % e)

        if nom_pivot in user.pivots.keys():
            abort(409, "Le pivot existe déjà pour cet utilisateur.")

        valeur_pivot = None
        try:
            valeur_pivot = request.get_json()['valeur_pivot']
        except Exception as e:
            abort(400, "Erreur lors de la récupération de la variable "
                       "'valeur_pivot': %s" % e)

        #Tout s'est bien passé, sauvegarde de l'utilisateur et retour info
        user.pivots[nom_pivot] = valeur_pivot
        user.save()

        return jsonify({"pivot": nom_pivot, "valeur": valeur_pivot})


    @app.route('/user/<courriel>/pivot', methods=['PUT'])
    def maj_pivot_user(courriel):
        """Mise à jour de la valeur d'un pivot pour le user.
        
        Attention, interdit de créer un nouveau pivot avec cette méthode.
        """

        user = None
        try:
            user = User.objects.get(email=courriel)
        except DoesNotExist:
            abort(404, "L'utilisateur n'existe pas.")

        # Validation du corps de la requête
        nom_pivot = None
        try:
            nom_pivot = request.get_json()['nom_pivot']
        except Exception as e:
            abort(400, "Erreur lors de la récupération de la variable "
                       "'nom_pivot': %s" % e)

        if nom_pivot not in user.pivots.keys():
            abort(400, "Le pivot n'existe pas encore pour cet utilisateur.")

        valeur_pivot = None
        try:
            valeur_pivot = request.get_json()['valeur_pivot']
        except Exception as e:
            abort(400, "Erreur lors de la récupération de la variable "
                       "'valeur_pivot': %s" % e)

        # Tout s'est bien passé, sauvegarde de l'utilisateur et retour info
        user.pivots[nom_pivot] = valeur_pivot
        user.save()

        return "Le pivot '%s' vaut désormais %s pour le user: %s" \
               % (nom_pivot, valeur_pivot, courriel)

    @app.route('/user/<courriel>', methods=['DELETE'])
    def delete_user(courriel):
        """Création d'un nouvel utilisateur avec son courriel"""
        try:
            user = User.objects.get(email=courriel)
        except DoesNotExist:
            abort(404, "Utilisateur introuvable")

        user.delete()
        return 'Suppression de l\'utilisateur: %s' % courriel
    */
}