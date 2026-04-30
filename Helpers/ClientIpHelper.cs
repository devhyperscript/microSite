namespace firstproject.Helpers
{
    public static class ClientIpHelper
    {
        /// <summary>
        /// Best-effort client IP: X-Forwarded-For first hop when present (reverse proxy), else connection remote address.
        /// </summary>
        public static string GetClientIp(HttpContext context)
        {
            var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwarded))
            {
                var first = forwarded.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .FirstOrDefault();
                if (!string.IsNullOrEmpty(first))
                    return first;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        }
    }
}
