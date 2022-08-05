using System.Linq.Expressions;

using Google.Cloud.Firestore;

namespace Firestore.Typed.Client;

public class TypedSetOptions<TDocument>
{
    public static readonly TypedSetOptions<TDocument> Overwrite = new(SetOptions.Overwrite);
    public static readonly TypedSetOptions<TDocument> MergeAll = new(SetOptions.MergeAll);

    internal SetOptions SetOptions { get; }

    public TypedSetOptions(SetOptions fields)
    {
        this.SetOptions = fields;
    }

    public static TypedSetOptions<TDocument> MergeFields<TField>(params Expression<Func<TDocument, TField>>[] fields)
    {
        string[] fieldMask = fields.Select(field => field.GetFieldName()).ToArray();
        return new TypedSetOptions<TDocument>(SetOptions.MergeFields(fieldMask));
    }
}