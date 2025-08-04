using Newtonsoft.Json.Linq;

namespace IndicoToolkit.Results;


public record Review
(
    int Id,
    int ReviewerId,
    string Notes,
    bool Rejected,
    ReviewType Type
) : IComparable<Review>
{
    public int CompareTo(Review other) => this.Id.CompareTo(other.Id);

    /*
    Determine the review type from its string representation.
    */
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

    /*
    Create a Review from a `reviews` list item.
    */
    public static Review FromJson(JToken json)
    {
        return new
        (
            Utils.Get<int>(json, "review_id"),
            Utils.Get<int>(json, "reviewer_id"),
            Utils.Get<string>(json, "review_notes"),
            Utils.Get<bool>(json, "review_rejected"),
            Review.ReviewTypeFromString(Utils.Get<string>(json, "review_type"))
        );
    }
};
