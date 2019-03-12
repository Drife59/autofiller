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
        public string cle { get; set; }
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
        public int first_name { get; set; }
        public int family_name { get; set; }
        public int postal_code { get; set; }
        public int home_city { get; set; }
        public int cellphone_number { get; set; }
        public int main_email { get; set; }
        public int main_full_address { get; set; }
        public int day_of_birth { get; set; }
        public int month_of_birth { get; set; }
        public int year_of_birth { get; set; }

        public int company { get; set; }
        public int homephone { get; set; }
        public int cvv { get; set; }
        public int cardexpirymonth { get; set; }
        public int cardexpiryyear { get; set; }

        public int full_birthdate { get; set; }
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
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }

        [JsonIgnore] 
        [IgnoreDataMember]
        public virtual ICollection<UserValue> UserValues { get; set; }
    }

    public class Profil
    {
        public long profilId { get; set; }
        public string profilName { get; set; }

        //Foreign Keys
        public virtual User User { get; set; }
        [Required]
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }

        [JsonIgnore] 
        [IgnoreDataMember]
        public virtual ICollection<UserValue> UserValues { get; set; }
    }
}