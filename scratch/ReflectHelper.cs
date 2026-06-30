using System;
using System.Reflection;
using System.Linq;

public class ReflectHelper
{
    public static void Main()
    {
        try
        {
            var assembly = Assembly.LoadFrom(@"E:\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord\bin\Win64_Shipping_Client\TaleWorlds.MountAndBlade.ViewModelCollection.dll");
            var itemType = assembly.GetType("TaleWorlds.MountAndBlade.ViewModelCollection.OrderOfBattle.OrderOfBattleFormationItemVM");

            Console.WriteLine("Scanning all assembly methods calling RefreshFormation...");
            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    var body = method.GetMethodBody();
                    if (body == null) continue;

                    byte[] il = body.GetILAsByteArray();
                    // Just scan the IL bytes or metadata to find if this method references RefreshFormation.
                    // A simple way is to check the method names or just print out methods that might be related.
                }
            }

            // Let's print out all method names in OrderOfBattleHeroItemVM
            var heroType = assembly.GetType("TaleWorlds.MountAndBlade.ViewModelCollection.OrderOfBattle.OrderOfBattleHeroItemVM");
            Console.WriteLine("\nMethods in OrderOfBattleHeroItemVM:");
            foreach (var m in heroType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                if (m.DeclaringType == heroType)
                {
                    Console.WriteLine($"- {m.Name}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex}");
        }
    }
}
