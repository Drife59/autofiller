using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Application_WEB_MVC.Controllers
{

    //Test avec personnalisation maximale d'adresse
    //[Route("koukou/[controller]/[action]")]
    [Route("/[controller]/[action]")]
    public class WebsiteController : Controller
    {
        //parametre yahou passable en argument ou dans l'url
        [Route("{yahou?}")]
        public string Index(int? yahou)
        {
            return "Koukou " + yahou;
        }

        [Route("{url_domaine}")]
        
        public string Url_domaine(string url_domaine)
        {
            return "Koukou " + url_domaine;
        }


    }

    /*
    @app.route('/domaine/<path:url_domaine>', methods=['GET'])
    def check_domaine_existance(url_domaine):
        """Vérifie que le domaine existe bien"""


    @app.route('/domaine/<path:url_domaine>', methods=['POST'])
    def new_domaine(url_domaine):
        """Création d'un nouveau domaine sur l'url concernée."""

    @app.route('/domaine/<path:url_domaine>/cles', methods=['GET'])
    def get_cles_domaines(url_domaine):
        """Retour des cles en paramètre"""

    # Il est important de différencier la création d'un nouveau pivot de sa
    # mise à jour. Ces dernières ne déclencherons pas le même algo.
    # POST = création, PUT = mise à jour

    @app.route('/domaine/<path:url_domaine>/pivot', methods=['POST'])
    def add_pivot_domaine(url_domaine):
        """Création d'un nouveau pivot pour le domaine."""

    @app.route('/domaine/<url_domaine>/pivot', methods=['PUT'])
    def maj_pivot_domaine(url_domaine):
        """Mise à jour de la valeur d'un pivot pour le domaine.

    @app.route('/domaine/<path:url_domaine>', methods=['DELETE'])
    def delete_domaine(url_domaine):
        """Suppression d'un domaine."""
        */
}