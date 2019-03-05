using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Bot.Builder.Azure;
using WomanDayBot.Models;

namespace WomanDayBot.Repositories
{
  public class CosmosDbRepository<T> where T : class
  {
    private readonly CosmosDbStorageOptions _configurationOptions;
    private readonly string _collectionId;
    private readonly string _databaseId;
    private readonly DocumentClient _client;

    public object DefaultOptions { get; private set; }

    public CosmosDbRepository(
      CosmosDbStorageOptions configurationOptions,
      string databaseId,
      string collectionId)
    {
      _configurationOptions = configurationOptions;
      _client = new DocumentClient(_configurationOptions.CosmosDBEndpoint, _configurationOptions.AuthKey);
      _collectionId = collectionId;
      _databaseId = databaseId;
    }

    public async Task<Order> GetItemAsync(string id)
    {
      var link = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
      var query = _client.CreateDocumentQuery<Order>(link, new SqlQuerySpec()
      {
        QueryText = "SELECT * FROM Orders f WHERE (f.orderId = @id)",
        Parameters = new SqlParameterCollection()
                    {
                        new SqlParameter("@id", id)
                    }
      }, new FeedOptions { EnableCrossPartitionQuery = true });
      return query.ToList().SingleOrDefault();
    }

    public async Task<IEnumerable<T>> GetItemsAsync()
    {
      using (var client = new DocumentClient(_configurationOptions.CosmosDBEndpoint, _configurationOptions.AuthKey))
      {
        var link = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
        var query = client.CreateDocumentQuery<T>(
            link,
            new FeedOptions()
            {
              EnableCrossPartitionQuery = true
            })
          .AsDocumentQuery();

        var results = new List<T>();
        while (query.HasMoreResults)
        {
          results.AddRange(await query.ExecuteNextAsync<T>());
        }
        return results;
      }
    }

    public async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate)
    {
      using (var client = new DocumentClient(_configurationOptions.CosmosDBEndpoint, _configurationOptions.AuthKey))
      {
        var link = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
        var query = client.CreateDocumentQuery<T>(
                            link,
                            new FeedOptions()
                            {
                              EnableCrossPartitionQuery = true
                            })
                         .Where(predicate)
                        .AsDocumentQuery();

        var results = new List<T>();
        while (query.HasMoreResults)
        {
          results.AddRange(await query.ExecuteNextAsync<T>());
        }
        return results;
      }
    }

    public async Task<Document> CreateItemAsync(T item)
    {
      return await _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId), item);
    }

    public async Task<Document> UpdateItemAsync(string id, Order item)
    {
      using (var client = new DocumentClient(_configurationOptions.CosmosDBEndpoint, _configurationOptions.AuthKey))
      {
        var link = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
        Document doc = client.CreateDocumentQuery<Document>(link, new FeedOptions { EnableCrossPartitionQuery = true })
          .Where(r => r.Id == id).AsEnumerable().SingleOrDefault();

        doc.SetPropertyValue("IsComplete", item.IsComplete);


        Document updated = await client.ReplaceDocumentAsync(doc);
        return updated;
      }
    }

    public async Task DeleteItemAsync(string id)
    {
      await _client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(_databaseId, _collectionId, id));
    }

    private async Task CreateDatabaseIfNotExistsAsync()
    {
      try
      {
        await _client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(_databaseId));
      }
      catch (DocumentClientException e)
      {
        if (e.StatusCode == HttpStatusCode.NotFound)
        {
          await _client.CreateDatabaseAsync(new Database { Id = _databaseId });
        }
        else
        {
          throw;
        }
      }
    }

    private async Task CreateCollectionIfNotExistsAsync()
    {
      try
      {
        await _client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId));
      }
      catch (DocumentClientException e)
      {
        if (e.StatusCode == HttpStatusCode.NotFound)
        {
          await _client.CreateDocumentCollectionAsync(
            UriFactory.CreateDatabaseUri(_databaseId),
            new DocumentCollection { Id = _collectionId },
            new RequestOptions { OfferThroughput = 1000 });
        }
        else
        {
          throw;
        }
      }
    }
  }
}
