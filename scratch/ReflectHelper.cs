using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

public class ReflectHelper3
{
    public static void Main()
    {
        try
        {
            var assembly = Assembly.LoadFrom(@"E:\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord\bin\Win64_Shipping_Client\TaleWorlds.MountAndBlade.ViewModelCollection.dll");
            
            // Get all RefreshFormation methods (regardless of signature)
            var itemType = assembly.GetType("TaleWorlds.MountAndBlade.ViewModelCollection.OrderOfBattle.OrderOfBattleFormationItemVM");
            var refreshMethods = itemType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => m.Name == "RefreshFormation").ToList();

            var coreAssembly = Assembly.LoadFrom(@"E:\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord\bin\Win64_Shipping_Client\TaleWorlds.Core.dll");
            var libAssembly = Assembly.LoadFrom(@"E:\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord\bin\Win64_Shipping_Client\TaleWorlds.Library.dll");
            var mbAssembly = Assembly.LoadFrom(@"E:\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord\bin\Win64_Shipping_Client\TaleWorlds.MountAndBlade.dll");

            Console.WriteLine("Scanning all assembly methods calling RefreshFormation...");
            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    var body = method.GetMethodBody();
                    if (body == null) continue;

                    byte[] il = body.GetILAsByteArray();
                    bool callsRefresh = false;
                    for (int i = 0; i < il.Length; i++)
                    {
                        byte op = il[i];
                        if (op == 0x28 || op == 0x6f) // call or callvirt
                        {
                            if (i + 4 < il.Length)
                            {
                                int token = BitConverter.ToInt32(il, i + 1);
                                try
                                {
                                    var resolved = type.Module.ResolveMethod(token);
                                    if (refreshMethods.Contains(resolved))
                                    {
                                        callsRefresh = true;
                                        break;
                                    }
                                }
                                catch {}
                            }
                        }
                    }

                    if (callsRefresh)
                    {
                        Console.WriteLine("- " + type.FullName + "." + method.Name);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.ToString());
        }
    }
}
