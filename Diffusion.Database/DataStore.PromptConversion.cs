using Diffusion.Database.Models;

namespace Diffusion.Database;

public partial class DataStore
{
    public int AddPromptConversion(PromptConversion conversion)
    {
        using var db = OpenConnection();

        lock (_lock)
        {
            db.Insert(conversion);
        }

        return conversion.Id;
    }

    public IReadOnlyList<PromptConversion> GetPromptConversions(int imageId)
    {
        using var db = OpenConnection();

        return db.Query<PromptConversion>(
            $"SELECT * FROM {nameof(PromptConversion)} WHERE ImageId = ? ORDER BY CreatedAt DESC, Id DESC",
            imageId);
    }
}
