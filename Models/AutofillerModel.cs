using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Autofiller.Models
{
    /* Object non persisted, only for front app communication in requests */

    public class PivotDomaineRequest
    {
        public string Cle { get; set; }
        public string Pivot { get; set; }
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
        [Required]
        public virtual Pivot Pivot { get; set; }
    }

    public class Pivot
    {
        public long pivotId { get; set; }
        [Required]
        public string name { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }

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

        //Foreign Keys
        public virtual Pivot Pivot { get; set; }
        public virtual User User { get; set; }
    }

    public class User
    {
        public long userId { get; set; }
        [Required]
        public string email { get; set; }
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
}