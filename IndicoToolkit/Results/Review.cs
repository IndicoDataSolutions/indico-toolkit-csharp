using Newtonsoft.Json.Linq;

namespace IndicoToolkit.Results;


public enum ReviewType
{
    ADMIN,
    AUTO,
    MANUAL
}


public class Review : PrettyPrint
{
    public int Id { get; init; }
    public int ReviewerId { get; init; }
    public string Notes { get; init; }
    public bool Rejected { get; init; }
    public ReviewType Type { get; init; }

    // Determine the review type from its string representation.
    public static ReviewType ReviewTypeFromString(string reviewType)
    {
        if (reviewType == "admin")
            return ReviewType.ADMIN;
        else if (reviewType == "auto")
            return ReviewType.AUTO;
        else if (reviewType == "manual")
            return ReviewType.MANUAL;
        else
            throw new ResultException($"unsupported review type `{reviewType}`");
    }

    // Create a Review from a `reviews` list item.
    public static Review FromJson(JToken json)
    {
        return new Review
        {
            Id = Utils.Get<int>(json, "review_id"),
            ReviewerId = Utils.Get<int>(json, "reviewer_id"),
            Notes = Utils.Get<string>(json, "review_notes"),
            Rejected = Utils.Get<bool>(json, "review_rejected"),
            Type = Review.ReviewTypeFromString(Utils.Get<string>(json, "review_type")),
        };
    }
};
