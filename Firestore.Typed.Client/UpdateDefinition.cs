using System.Linq.Expressions;

namespace Firestore.Typed.Client;

public class UpdateDefinition<TDocument>
{
    internal IDictionary<string, object?> UpdateValues { get; } = new Dictionary<string, object?>();

    public UpdateDefinition<TDocument> Set<TField>(Expression<Func<TDocument, TField>> field, TField? value)
    {
        string fieldName = field.GetField();
        if (UpdateValues.TryAdd(fieldName, value))
        {
            return this;
        }

        UpdateValues[fieldName] = value;
        return this;
    }
}