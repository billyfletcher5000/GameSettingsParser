using System.Reflection;

namespace GameSettingsParser.Utility
{
    public static class ReflectionHelper
    {
        public static IReadOnlyList<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (Exception? loaderException in ex.LoaderExceptions)
                {
                    if (loaderException is not null)
                    {
                        Console.WriteLine(loaderException.Message);
                    }
                }

                return ex.Types
                    .Where(type => type is not null)
                    .Cast<Type>()
                    .ToArray();
            }
        }
    }
}