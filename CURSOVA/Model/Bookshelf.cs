namespace CURSOVA.Model
{
    public class Bookshelf
    {

            public string kind { get; set; }
            public int id { get; set; }
            public string selfLink { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public string access { get; set; }
            public DateTime updated { get; set; }
            public DateTime created { get; set; }
            public int volumeCount { get; set; }
            public DateTime volumesLastUpdated { get; set; }
        

    }
}
