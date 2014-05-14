﻿using MongoDB.Driver;
using MongoDB.Driver.Linq;
using PodcastAggregator.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace PodcastAggregator.DB
{
    public class DataContext
    {
        public MongoDatabase Database;
        public MongoCollection<Producer> Producers { get { return Database.GetCollection<Producer>("producers"); } }
        public MongoCollection<Show> Shows { get { return Database.GetCollection<Show>("shows"); } }

        public DataContext()
        {
            var serverAndDb = new ServerAndDb(GetConnectionString());
            var client = new MongoClient(serverAndDb.Server);
            var server = client.GetServer();
            Database = server.GetDatabase(serverAndDb.Database);
        }

        #region Api

        public IEnumerable<Producer> ActiveProducers()
        {
            return Producers.FindAll().Where(x => x.Active == true).OrderBy(x => x.Name);
        }

        public IQueryable<Show> ActiveShows(string search)
        {
            var shows = Shows.AsQueryable()
                        .Where(x => x.Active);

            if (!string.IsNullOrWhiteSpace(search))
                shows = shows.Where(x => x.Title.ToLower().Contains(search.ToLower()) || x.Description.ToLower().Contains(search.ToLower())).AsQueryable();

            return shows.OrderByDescending(x => x.PublicationDate);
        }

        public void DeleteShows()
        {
            this.Shows.RemoveAll();
        }

        #endregion

        #region Utils
        private string GetConnectionString()
        {

            return WebConfigurationManager.AppSettings.Get("MONGOHQ_URL") ??
                  WebConfigurationManager.AppSettings.Get("MONGOLAB_URI") ??
                  WebConfigurationManager.ConnectionStrings["MongoDBConnection"].ConnectionString;
        }

        #endregion
    }


    class ServerAndDb
    {
        public string Server { get; set; }
        public string Database { get; set; }

        public ServerAndDb(string fullConnectionString)
        {
            if (!string.IsNullOrEmpty(fullConnectionString))
            {
                var slashIndex = fullConnectionString.LastIndexOf('/');
                Server = fullConnectionString.Substring(0, slashIndex);
                if (slashIndex < fullConnectionString.Length)
                    Database = fullConnectionString.Substring(++slashIndex);
            }

        }
    }
}