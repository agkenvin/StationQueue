using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StationQueue.Models
{
    public class MongoDBSettings
    {
       public string ConnectionURI {get; set;} = null!;
       public string DatabaseName {get; set;} = null!;
       public string QueueModelCollection {get; set;} = null!;
    }
}