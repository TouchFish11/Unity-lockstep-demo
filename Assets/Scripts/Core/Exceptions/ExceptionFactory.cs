namespace Core.Exceptions
{
    /// <summary>
    /// 异常工厂
    /// </summary>
    public static class ExceptionFactory
    {
        public static System.Exception AssetNullException(string assetName, System.Exception innerException = null)
        {
            return new System.Exception($"Load {assetName} is Null", innerException);
        }
    }
}
