using GameSettingsParser.Attributes;

namespace GameSettingsParser.Utility
{
    public static class SwitchableServiceHelper
    {
        public static IEnumerable<Type> GetSwitchableServiceImplementations<T>() where T : class
        {
            var types = new List<Type>();
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                var assemblyTypes = assembly.GetTypes();

                foreach (var type in assemblyTypes)
                {
                    if(type.IsAssignableFrom(typeof(T)) && Attribute.GetCustomAttribute(type, typeof(SwitchableServiceAttribute)) is SwitchableServiceAttribute)
                        types.Add(type);
                }
            }

            return types;
        }
        
        public static string? GetDefaultSwitchableServiceId<T>() where T : class
        {
            var types = GetSwitchableServiceImplementations<T>();
            foreach (var type in types)
            {
                var attribute = Attribute.GetCustomAttribute(type, typeof(SwitchableServiceAttribute)) as SwitchableServiceAttribute;
                if (attribute?.IsDefault == true)
                    return attribute.ServiceId; 
            }
            
            return null;
        }
        
        public static string? GetSwitchableServiceId<T>() where T : class
        {
            var attribute = Attribute.GetCustomAttribute(typeof(T), typeof(SwitchableServiceAttribute)) as SwitchableServiceAttribute;
            return attribute?.ServiceId;
        }
        
        public static string? GetSwitchableServiceId(Type type)
        {
            var attribute = Attribute.GetCustomAttribute(type, typeof(SwitchableServiceAttribute)) as SwitchableServiceAttribute;
            return attribute?.ServiceId;
        } 
        
        public static string? GetSwitchableServiceDisplayName<T>() where T : class
        {
            var attribute = Attribute.GetCustomAttribute(typeof(T), typeof(SwitchableServiceAttribute)) as SwitchableServiceAttribute;
            return attribute?.DisplayName;
        }
        
        public static string? GetSwitchableServiceDisplayName(Type type)
        {
            var attribute = Attribute.GetCustomAttribute(type, typeof(SwitchableServiceAttribute)) as SwitchableServiceAttribute;
            return attribute?.DisplayName;
        }
    }
}