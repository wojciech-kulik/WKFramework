namespace WKFramework.Utils
{
    public interface IBinaryConverter
    {
        byte[] ConvertToBinary(object obj);

        object ConvertFromBinary(byte[] data);
    }
}
