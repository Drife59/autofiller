namespace Website.Models
{
    //Object "Pivot website" for front app communication
    public class PivotDomaineRequest
    {
        public string Cle { get; set; }
        public string Pivot { get; set; }
    }

    //Object "Pivot user" for front app communication
    public class PivotUserRequest
    {
        public string Pivot { get; set; }
        public string Value { get; set; }
    }

    //Object "Merge request" for front app communication
    public class MergeRequest
    {
        public string email { get; set; }
        public string domaine { get; set; }
        public string cle_req { get; set; }
        public string valeur { get; set; }
    }
}