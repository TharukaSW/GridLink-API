namespace GridLink.Models
{
    public class MongoDBSetting
    {
        public String ConnectionString { get; set; } = string.Empty; 
        public String DatabaseName { get; set;} = string.Empty;
        public String UserCollectioNames { get; set; } = "Users";
    }
}
