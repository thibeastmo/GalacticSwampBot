using System;
using System.Drawing;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace GalacticSwampBot.Library.Database {
    
    public class Colony {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string _player_id { get; set; }
        public int id_gl { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string _alliance_id { get; set; }
        public short number { get; set; }
        public string colo_sys_name { get; set; }
        public int colo_lvl { get; set; }
        public Coordinates colo_coord { get; set; }
        public string colo_status { get; set; }
        public DateTime colo_last_attack_time { get; set; }
        public DateTime colo_refresh_time { get; set; }
        public bool updated { get; set; } = true;
        public bool scouted { get; set; }
        public string gift_state { get; set; }
        public string bunker_troops { get; set; }
    }
}
