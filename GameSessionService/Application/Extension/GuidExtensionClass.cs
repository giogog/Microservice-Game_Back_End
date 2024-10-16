namespace GameSession.Application.Extension
{
    public static class GuidExtensionClass
    {
        public static Guid ToGuid(this string prop)
        {
            Guid.TryParse(prop, out Guid guid);
            return guid;
        }

    }
}
