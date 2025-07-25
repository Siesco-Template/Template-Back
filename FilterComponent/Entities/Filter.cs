using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterComponent.Entities
{
    public class Filter
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonRequired]
        public string UserId { get; set; }
        /// <summary>
        /// Hansı table-nin filteri olduğunu yadda saxlamaq üçün
        /// </summary>
        public string TableId { get; set; }
        /// <summary>
        /// Yadda saxlanılan filterin daxil edilən adı
        /// </summary>
        public string FilterTitle { get; set; }
        /// <summary>
        /// Verilən value-lara görə formalaşan query
        /// </summary>
        public string? FilterQuery { get; set; }
        /// <summary>
        /// Filterin yaradılması üçün queryler
        /// </summary>
        public List<FilterKeyValue> FilterValues { get; set; }
        /// <summary>
        /// Filterin yaradıldığı tarix
        /// </summary>
        public DateTime CreatedAt { get; set; } 
        /// <summary>
        /// Table-ın default propertysi olması
        /// </summary>
        public bool IsDefault { get; set; }
        
    }
}
