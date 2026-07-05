using System.Reflection;

namespace ACI.Helper
{
    public static class GeneralHelperService
    {
        public static List<string> GetPropertyNameList(string typeName)
        {
            Type type = Type.GetType(typeName);
            if (type == null)
            {
                type = Assembly.GetExecutingAssembly().GetType(typeName);
            }
            if (type != null)
            {
                return type.GetProperties().Select(p => p.Name).ToList();
            }
            return new List<string>();
        }

        public static object ConvertToType(string value, Type targetType)
        {
            try
            {
                if (string.IsNullOrEmpty(value)) return null;
                return Convert.ChangeType(value, targetType);
            }
            catch
            {
                return null;
            }
        }
    }
}
