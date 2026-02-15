using System.Text.Json;
using ProductHub.Web.Models;

namespace ProductHub.Web.Services;

public class CartSessionStore
{
    public const string SessionKey = "store-cart";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public IReadOnlyList<CartItemModel> Get(ISession session)
    {
        var payload = session.GetString(SessionKey);
        if (string.IsNullOrWhiteSpace(payload))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<CartItemModel>>(payload, JsonOptions) ?? [];
    }

    public void Save(ISession session, IReadOnlyList<CartItemModel> cartItems)
    {
        var payload = JsonSerializer.Serialize(cartItems, JsonOptions);
        session.SetString(SessionKey, payload);
    }

    public void Clear(ISession session)
    {
        session.Remove(SessionKey);
    }
}
