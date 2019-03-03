using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Bot.Builder.Azure;

namespace WomanDayBot.Orders
{
  public class OrderRepository<T> where T : class
  {
    private static readonly string DatabaseId = "WomanDayBot";
    private static readonly string CollectionId = "Orders";
    private readonly CosmosDbStorageOptions _configurationOptions;
    private static DocumentClient client;

    public OrderRepository(CosmosDbStorageOptions configurationOptions)
    {
      _configurationOptions = configurationOptions;
      client = new DocumentClient(_configurationOptions.CosmosDBEndpoint, _configurationOptions.AuthKey);
    }

    public async Task<T> GetItemAsync(string orderId)
    {
      try
      {
        Document document = await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, orderId));
        return (T)(dynamic)document;
      }
      catch (DocumentClientException e)
      {
        if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
          return null;
        }
        else
        {
          throw;
        }
      }
      catch (Exception ex)
      {
        var t = ex;
        throw;
      }
    }

    public async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate)
    {
      IDocumentQuery<T> query = client.CreateDocumentQuery<T>(
          UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId),
          new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true })
          .Where(predicate)
          .AsDocumentQuery();

      List<T> results = new List<T>();
      while (query.HasMoreResults)
      {
        results.AddRange(await query.ExecuteNextAsync<T>());
      }

      return results;
    }

    public async Task<Document> CreateItemAsync(T item)
    {
      return await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId), item);
    }

    public async Task<Document> UpdateItemAsync(string id, T item)
    {
      return await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id), item);
    }

    public async Task DeleteItemAsync(string id)
    {
      await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id));
    }


    private async Task CreateDatabaseIfNotExistsAsync()
    {
      try
      {
        await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(DatabaseId));
      }
      catch (DocumentClientException e)
      {
        if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
          await client.CreateDatabaseAsync(new Database { Id = DatabaseId });
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
        await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId));
      }
      catch (DocumentClientException e)
      {
        if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
          await client.CreateDocumentCollectionAsync(
              UriFactory.CreateDatabaseUri(DatabaseId),
              new DocumentCollection { Id = CollectionId },
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
