using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleBot.dto
{
    public class Post
    {
        public Guid Id { get; set; }
        public DateTime TravelDateStart { get; set; }
        public DateTime TravelDateEnd { get; set; }
        public string Description { get; set; }
        public byte[] Picture { get; set; }
        public string Link { get; set; }
        public string Status { get; set; }
        public bool IsVip { get; set; }
        public bool IsVipRequested { get; set; }

        public Post(Guid id, DateTime dateTimeStart, DateTime dateTimeEnd, string discription, string link)
        {
            Id = id;
            TravelDateStart = dateTimeStart;
            TravelDateEnd = dateTimeEnd;
            Description = discription;
            Link = link;
            Status = "Планируется";
        }
    }
}
