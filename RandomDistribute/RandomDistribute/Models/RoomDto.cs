using System.Collections.Generic;

namespace RandomDistribute.Models
{
    public class RoomDto
    {
        public string RoomOwner { get; set; }
        public IDictionary<string, string> Users { get; set; }

        public RoomDto()
        {
            Users = new Dictionary<string, string>();
        }
    }
}