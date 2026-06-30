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
            var vmType = assembly.GetType("TaleWorlds.MountAndBlade.ViewModelCollection.OrderOfBattle.OrderOfBattleVM");

            Console.WriteLine("Reflecting VM methods...");
            foreach (var method in vmType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                var body = method.GetMethodBody();
                if (body == null) continue;

                // We can read IL bytes and search for calls
                byte[] il = body.GetILAsByteArray();
                // We want to find any Call or Callvirt to a method named RefreshFormation
                // Let's print out the method body IL metadata to find calls
                // Or simply search for the method reference in the assembly module
                // Actually, let's inspect the instructions using a basic IL reader or by checking the method names referenced
            }

            // Let's look at the methods in OrderOfBattleVM that reference OrderOfBattleFormationItemVM
            foreach (var method in vmType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                var body = method.GetMethodBody();
                if (body == null) continue;
                
                // Let's print the local variables and their types
                var locals = body.LocalVariables;
                bool referencesItem = locals.Any(l => l.LocalType == itemType);
                if (referencesItem)
                {
                    Console.WriteLine($"Method referencing OrderOfBattleFormationItemVM: {method.Name}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex}");
        }
    }
}
