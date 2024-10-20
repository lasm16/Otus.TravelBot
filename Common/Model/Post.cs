namespace Common.Model
{
    public class Post
    {
        public Guid Id { get; set; }
        public DateTime TravelDateStart { get; set; }
        public DateTime TravelDateEnd { get; set; }
        public string Description { get; set; }
        public byte[] Picture { get; set; }
        public string LinkToVk { get; set; }
        public string Status { get; set; }
        public bool IsVip { get; set; }
        public bool IsVipRequested { get; set; }

        public Post(Guid id, DateTime dateTimeStart, DateTime dateTimeEnd, string discription, string link)
        {
            Id = id;
            TravelDateStart = dateTimeStart;
            TravelDateEnd = dateTimeEnd;
            Description = discription;
            LinkToVk = link;
            Status = "Планируется";
        }
    }
}
