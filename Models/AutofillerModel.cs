using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Autofiller.Models
{
    /* Object non persisted, only for front app communication in requests */

    public class KeyRequest
    {
        public string Cle { get; set; }
        public string pivot_reference { get; set; }

        public string first_name { get; set; }
        public string family_name { get; set; }
        public string postal_code { get; set; }
        public string home_city { get; set; }
        public string cellphone_number { get; set; }
        public string main_email { get; set; }
        public string main_full_address { get; set; }
        public string day_of_birth { get; set; }
        public string month_of_birth { get; set; }
        public string year_of_birth { get; set; }

        public string company { get; set; }
        public string homephone { get; set; }
        public string cvv { get; set; }
        public string cardexpirymonth { get; set; }
        public string cardexpiryyear { get; set; }

        public string full_birthdate { get; set; }
    }

    public class PivotUserRequest
    {
        public string Pivot { get; set; }
        public string Value { get; set; }
    }

    public class MergeRequest
    {
        public string email { get; set; }
        public string domaine { get; set; }
        public string cle_req { get; set; }
        public string valeur { get; set; }
    }

    /* Object persisted, main DB */

    public class Website
    {
        public long websiteId { get; set; }
        [Required]
        public string domaine { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }

        [JsonIgnore] 
        [IgnoreDataMember]
        public virtual ICollection<Key> Keys { get; set; } 
    }

    public class Key
    {
        public long keyId { get; set; }
        [Required]
        public string code { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        [Required]
        public virtual Website Website { get; set; }
        
        //This can be null
        public virtual Pivot Pivot { get; set; }

        //Define weight pivot
        public long first_name { get; set; }
        public long family_name { get; set; }
        public long postal_code { get; set; }
        public long home_city { get; set; }
        public long cellphone_number { get; set; }
        public long main_email { get; set; }
        public long main_full_address { get; set; }
        public long day_of_birth { get; set; }
        public long month_of_birth { get; set; }
        public long year_of_birth { get; set; }

        public long company { get; set; }
        public long homephone { get; set; }
        public long cvv { get; set; }
        public long cardexpirymonth { get; set; }
        public long cardexpiryyear { get; set; }

        public long full_birthdate { get; set; }
    }

    public class Pivot
    {
        public long pivotId { get; set; }
        [Required]
        public string name { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public Boolean restitution_enabled { get; set; }

        //ICollection of foreign Key
        [JsonIgnore] 
        [IgnoreDataMember]
        public virtual ICollection<UserValue> UserValues { get; set; }
        [JsonIgnore] 
        [IgnoreDataMember]
        public virtual ICollection<Key> Keys { get; set; }
    }

    public class UserValue
    {
        public long userValueId { get; set; }
        [Required]
        public string value { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public decimal weight { get; set; }

        //Foreign Keys
        public virtual Pivot Pivot { get; set; }
        public virtual User User { get; set; }
    }

    public class User
    {
        public long userId { get; set; }
        [Required]
        public string email { get; set; }
        public string password_hash { get; set; }
        public string family_name { get; set; }
        public string surname { get; set; }
        public string phone { get; set; }
        public string postal_code { get; set; }
        public string city { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }

        [JsonIgnore] 
        [IgnoreDataMember]
        public virtual ICollection<UserValue> UserValues { get; set; }
    }

    public class Merge
    {
        public long mergeId { get; set; }
        public User User { get; set; }
        public Website Website { get; set; }
        public Pivot PivotFromUser { get; set; }
        public Pivot PivotFromWebsite { get; set; }
        [Required]
        public DateTime created_at { get; set; }
        public DateTime treated_at { get; set; } 
        public string status { get; set; }

        public const string STATUS_NEW = "New";
        public const string STATUS_VALIDATED = "Validated";
        public const string STATUS_REFUSED = "Refused";
    }
}