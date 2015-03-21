namespace WKFramework.Utils.Serializer
{
    public interface IConverter<TConverted>
    {
        TConverted Convert<TSource>(TSource obj);

        TResult ConvertFrom<TResult>(TConverted data);
    }
}
