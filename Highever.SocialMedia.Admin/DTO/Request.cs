namespace Highever.SocialMedia.Admin
{
    public record TikHubSearchRequest(
        string keyword,
        int offset = 0,
        int count = 20,
        int sort_type = 0,
        int publish_time = 0);
}
