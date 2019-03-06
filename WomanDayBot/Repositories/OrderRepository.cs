using Microsoft.Bot.Builder.Azure;
using WomanDayBot.Models;

namespace WomanDayBot.Repositories
{
  using System.Linq;
  using System.Threading.Tasks;

  using Microsoft.Azure.Documents;
  using Microsoft.Azure.Documents.Client;

  public class OrderRepository : CosmosDbRepository<Order>
  {
    private const string DatabaseId = "WomanDayBot";
    private const string CollectionId = "Orders";

    public OrderRepository(CosmosDbStorageOptions configurationOptions) 
      : base(configurationOptions, DatabaseId, CollectionId) { }

    /// <summary>
    /// Patches the item asynchronous.
    /// </summary>
    /// <typeparam name="T">Type of property</typeparam>
    /// <param name="id">The identifier.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="item">The item.</param>
    /// <returns>Updated item</returns>
    public async Task<Document> PatchItemAsync<T>(string id, string propertyName, T item)
    {
      using (var client = new DocumentClient(_configurationOptions.CosmosDBEndpoint, _configurationOptions.AuthKey))
      {
        var link = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
        Document doc = client.CreateDocumentQuery<Document>(link, new FeedOptions { EnableCrossPartitionQuery = true })
          .Where(r => r.Id == id).AsEnumerable().SingleOrDefault();

        doc.SetPropertyValue(propertyName, item);

        Document updated = await client.ReplaceDocumentAsync(doc);
        return updated;
      }
    }
  }
}
