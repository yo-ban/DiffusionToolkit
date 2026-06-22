using Diffusion.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Diffusion.Common.Query;

namespace Diffusion.Database
{
    public partial class DataStore
    {
        public bool QueryExists(string name)
        {
            using var db = OpenConnection();

            var count = db.ExecuteScalar<int>("SELECT COUNT(1) FROM Query WHERE Name = ?", name);
            return count == 1;
        }

        public void CreateOrUpdateQuery(string name, QueryOptions queryOptions)
        {
            var json = JsonSerializer.Serialize(queryOptions);

            using var db = OpenConnection();

            var command = db.CreateCommand("REPLACE INTO Query (Name, QueryJson, CreatedDate) VALUES (?,?,?)", name, json, DateTime.Now);

            lock (_lock)
            {
                command.ExecuteNonQuery();
            }
        }


        public IEnumerable<QueryItem> GetQueries()
        {
            using var db = OpenConnection();

            var items = db.Query<QueryItem>("SELECT Id, Name, CreatedDate FROM Query ORDER BY Name DESC");
            return items;
        }

        public QueryOptions GetQuery(int id)
        {

            using var db = OpenConnection();

            var queries = db.Query<Query>("SELECT Name, QueryJson, CreatedDate FROM Query WHERE Id = @Id", id);

            var options = JsonSerializer.Deserialize<QueryOptions>(queries[0].QueryJson);
            return options;
        }

        public void RenameQuery(int id, string name)
        {
            using var db = OpenConnection();

            var command = db.CreateCommand("UPDATE Query SET Name = ?, ModifiedDate = ? WHERE Id = ?", name, DateTime.Now, id);

            lock (_lock)
            {
                command.ExecuteNonQuery();
            }
        }

        public void RemoveQuery(int id)
        {
            using var db = OpenConnection();

            var command = db.CreateCommand("DELETE FROM Query WHERE Id = ?", id);

            lock (_lock)
            {
                command.ExecuteNonQuery();
            }
        }

    }
}
