using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using MySqlConnector;
namespace GalacticSwampBot.Library.Database {
    public class DatabaseHandler {
        public const string connectionString = "";
        public const string databaseName = "Galactic-Swamp";
        public const string collectionName = "colonies";
        public static async Task<bool> AddColony(Colony newColony)
        {
            try{
                var client = new MongoClient(connectionString);
                var db = client.GetDatabase(databaseName);
                var collection = db.GetCollection<Colony>(collectionName);
                await collection.InsertOneAsync(newColony);
            }
            catch (Exception ex){
                return false;
            }
            return true;
        }
        public static async Task<bool> FindAndReplace(Colony newColony)
        {
            try{
                var client = new MongoClient(connectionString);
                var db = client.GetDatabase(databaseName);
                var collection = db.GetCollection<Colony>(collectionName);
                await collection.FindOneAndReplaceAsync(c => c._id == newColony._id, newColony);
            }
            catch (Exception ex){
                return false;
            }
            return true;
        }
        public static async Task<List<Colony>> GetStoredColonies(int id_gl)
        {
            try{
                var client = new MongoClient(connectionString);
                var db = client.GetDatabase(databaseName);
                var collection = db.GetCollection<Colony>(collectionName);
                return (await collection.FindAsync(c => c.id_gl == id_gl)).ToList();
            }
            catch (Exception ex){
                return null;
            }
        }
    }
}
