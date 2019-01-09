using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EveEntrepreneurWebPersistency3.Models
{
    public class CacheTimer
    {
        [Key, Column(Order = 0), Index("CacheTimer", IsUnique = true, Order = 0)]
        public string Resource { get; set; }

        [Key, Column(Order = 1), Index("CacheTimer", IsUnique = true, Order = 1)]
        public string DataSource { get; set; }

        [Key, Column(Order = 2), Index("CacheTimer", IsUnique = true, Order = 2)]
        public string Key { get; set; }

        public DateTime Expires { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}