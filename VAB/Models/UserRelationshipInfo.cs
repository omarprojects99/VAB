using System.ComponentModel.DataAnnotations;

namespace VAB.Models
{
    public class UserRelationshipInfo
    {
        public string UserId { get; set; }
        public bool HasRelatedData { get; set; }
        public List<string> RelationshipDetails { get; set; } = new List<string>();
        public int TotalRelationships => RelationshipDetails.Count;
    }
}
