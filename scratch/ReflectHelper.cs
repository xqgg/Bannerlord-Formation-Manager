using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

public class ReflectHelper5
{
    public static void Main()
    {
        try
        {
            var assembly = Assembly.LoadFrom(@"E:\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord\bin\Win64_Shipping_Client\TaleWorlds.MountAndBlade.ViewModelCollection.dll");
            var coreVMAssembly = Assembly.LoadFrom(@"E:\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord\bin\Win64_Shipping_Client\TaleWorlds.Core.ViewModelCollection.dll");
            
            var itemType = assembly.GetType("TaleWorlds.MountAndBlade.ViewModelCollection.OrderOfBattle.OrderOfBattleFormationClassSelectorItemVM");
            var selectorGenericType = coreVMAssembly.GetType("TaleWorlds.Core.ViewModelCollection.Selector.SelectorVM`1");
            if (selectorGenericType == null)
            {
                Console.WriteLine("SelectorVM`1 type not found!");
                return;
            }
            var selectorType = selectorGenericType.MakeGenericType(itemType);
            var setIndexMethod = selectorType.GetMethod("set_SelectedIndex");
            if (setIndexMethod == null)
            {
                Console.WriteLine("set_SelectedIndex method not found!");
                return;
            }

            Console.WriteLine("Scanning all assembly methods calling set_SelectedIndex...");
            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    var body = method.GetMethodBody();
                    if (body == null) continue;

                    byte[] il = body.GetILAsByteArray();
                    bool callsSetIndex = false;
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
                                    if (resolved == setIndexMethod || (resolved.Name == "set_SelectedIndex" && resolved.DeclaringType.FullName.Contains("SelectorVM")))
                                    {
                                        callsSetIndex = true;
                                        break;
                                    }
                                }
                                catch {}
                            }
                        }
                    }

                    if (callsSetIndex)
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
